using MissionPlanner.Controls;
using MissionPlanner.Utilities;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;

namespace MissionPlanner.GCSViews
{
    /// <summary>
    /// AI Chat Assistant form for Mission Planner
    /// </summary>
    public partial class ChatAssistant : MyUserControl, IActivate
    {
        private AIBackendService aiService;
        private DroneCommandExecutor commandExecutor;
        private TelemetryCollector telemetryCollector;
        private bool isProcessing = false;
        private bool isConnected = false;
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Constructor for ChatAssistant form
        /// </summary>
        public ChatAssistant()
        {
            InitializeComponent();
            
            // Initialize AI backend service
            aiService = new AIBackendService("http://localhost:5000", 30);
            
            // Initialize command executor with Mission Planner's MAVLink connection
            commandExecutor = new DroneCommandExecutor(MainV2.comPort);
            
            // Initialize telemetry collector
            telemetryCollector = new TelemetryCollector(MainV2.comPort);
            
            // Set default mode to Ask (read-only) for safety
            modeComboBox.SelectedIndex = 1;  // Ask mode (read-only)
            
            // Add mode change event handler
            modeComboBox.SelectedIndexChanged += ModeComboBox_SelectedIndexChanged;
            
            // Load available models from Ollama
            LoadAvailableModels();
        }

        /// <summary>
        /// Handles mode selection changes and shows warning for Agent mode
        /// </summary>
        private void ModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // If switching to Agent mode (index 0), show warning
            if (modeComboBox.SelectedIndex == 0)
            {
                AppendMessage("[System] ⚠️ WARNING: Agent Mode enabled. AI can now control drone functions including ARM, TAKEOFF, LAND, and movement commands. Use with caution!", Color.FromArgb(255, 165, 0));
            }
            else
            {
                AppendMessage("[System] ✓ Ask Mode enabled. AI is in read-only mode and cannot execute commands.", Color.FromArgb(0, 200, 83));
            }
        }

        /// <summary>
        /// Called when the form is activated
        /// </summary>
        public void Activate()
        {
            // Apply theme to controls
            ThemeManager.ApplyThemeTo(this);
        }

        /// <summary>
        /// Handles the Send button click event
        /// </summary>
        private void sendButton_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        /// <summary>
        /// Handles the Enter key press in the input text box
        /// </summary>
        private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                SendMessage();
            }
        }

        /// <summary>
        /// Sends the user message and displays the response
        /// </summary>
        private async void SendMessage()
        {
            try
            {
                // Prevent multiple simultaneous requests
                if (isProcessing)
                {
                    return;
                }

                string userMessage = inputTextBox.Text.Trim();

                if (string.IsNullOrEmpty(userMessage))
                {
                    return;
                }

                // Mark as processing - hide send, show cancel
                isProcessing = true;
                sendButton.Visible = false;
                
                // Create cancellation token and show cancel button
                cancellationTokenSource = new CancellationTokenSource();
                cancelButton.Visible = true;

                // Display user message in modern blue
                AppendMessage("You: " + userMessage, Color.FromArgb(0, 120, 215));

                // Clear input box
                inputTextBox.Clear();

                // Show loading indicator
                AppendMessage("Assistant: Thinking...", Color.Gray);

                // Auto-scroll to bottom
                chatHistoryBox.SelectionStart = chatHistoryBox.Text.Length;
                chatHistoryBox.ScrollToCaret();

                // Get mode and model from UI
                string mode = modeComboBox.SelectedItem?.ToString().ToLower() ?? "agent";
                string model = modelComboBox.SelectedItem?.ToString() ?? "qwen2.5:3b";
                
                // Collect telemetry data
                var telemetry = telemetryCollector.CollectAll();

                // Get AI response with mode, model, telemetry, and cancellation token
                AIResponse aiResponse = await aiService.SendMessageAsync(
                    userMessage, 
                    mode, 
                    model, 
                    telemetry,
                    cancellationTokenSource.Token
                );

                // Remove "Thinking..." message
                RemoveLastMessage();

                // Check if request was successful
                if (!aiResponse.Success)
                {
                    AppendMessage($"Assistant: [Error: {aiResponse.Error}]", Color.Red);
                    chatHistoryBox.SelectionStart = chatHistoryBox.Text.Length;
                    chatHistoryBox.ScrollToCaret();
                    return;
                }

                // Display AI response
                AppendMessage("Assistant: " + aiResponse.Response, Color.Black);

                // Execute command if present
                if (aiResponse.Command != null)
                {
                    AppendMessage($"[Executing: {aiResponse.Command.Type}...]", Color.Blue);
                    
                    string result = await commandExecutor.ExecuteCommand(aiResponse.Command);
                    
                    // Color code the result (green for success, red for error)
                    Color resultColor = result.StartsWith("✓") ? Color.Green : Color.Red;
                    AppendMessage(result, resultColor);
                }

                // Auto-scroll to bottom
                chatHistoryBox.SelectionStart = chatHistoryBox.Text.Length;
                chatHistoryBox.ScrollToCaret();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error sending message: " + ex.Message, Strings.ERROR);
            }
            finally
            {
                // Re-enable send button and hide cancel button
                isProcessing = false;
                sendButton.Visible = true;
                cancelButton.Visible = false;
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Handles cancel button click to stop AI processing
        /// </summary>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Cancel the ongoing request
                cancellationTokenSource?.Cancel();
                
                // Remove "Thinking..." message
                RemoveLastMessage();
                
                // Show cancellation message
                AppendMessage("Assistant: [Request cancelled by user]", Color.Orange);
                
                // Auto-scroll to bottom
                chatHistoryBox.SelectionStart = chatHistoryBox.Text.Length;
                chatHistoryBox.ScrollToCaret();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error cancelling request: " + ex.Message, Strings.ERROR);
            }
        }

        /// <summary>
        /// Appends a message to the chat history with the specified color
        /// </summary>
        /// <param name="message">The message to append</param>
        /// <param name="color">The color of the message text</param>
        private void AppendMessage(string message, Color color)
        {
            chatHistoryBox.SelectionStart = chatHistoryBox.Text.Length;
            chatHistoryBox.SelectionLength = 0;
            
            // Adjust colors for dark background readability
            Color displayColor = color;
            if (color == Color.Black)
                displayColor = Color.FromArgb(230, 230, 230);
            else if (color == Color.Gray)
                displayColor = Color.FromArgb(150, 150, 150);
            else if (color == Color.Blue)
                displayColor = Color.FromArgb(100, 180, 255);
            else if (color == Color.Green)
                displayColor = Color.FromArgb(100, 255, 100);
            else if (color == Color.Red || color == Color.OrangeRed)
                displayColor = Color.FromArgb(255, 120, 120);
            else if (color == Color.FromArgb(0, 120, 215))
                displayColor = Color.FromArgb(100, 200, 255);
            
            chatHistoryBox.SelectionColor = displayColor;
            chatHistoryBox.SelectionFont = new Font(chatHistoryBox.Font.FontFamily, 12, FontStyle.Regular);
            chatHistoryBox.AppendText(message + Environment.NewLine + Environment.NewLine);
            chatHistoryBox.SelectionColor = chatHistoryBox.ForeColor;
        }

        /// <summary>
        /// Removes the last message from the chat history (used to remove loading indicator)
        /// </summary>
        private void RemoveLastMessage()
        {
            try
            {
                string text = chatHistoryBox.Text;
                int lastDoubleNewline = text.LastIndexOf(Environment.NewLine + Environment.NewLine);
                
                if (lastDoubleNewline > 0)
                {
                    // Find the previous double newline to get the start of the last message
                    int previousDoubleNewline = text.LastIndexOf(Environment.NewLine + Environment.NewLine, lastDoubleNewline - 1);
                    int startPos = previousDoubleNewline >= 0 ? previousDoubleNewline + (Environment.NewLine + Environment.NewLine).Length : 0;
                    
                    chatHistoryBox.Select(startPos, chatHistoryBox.Text.Length - startPos);
                    chatHistoryBox.SelectedText = "";
                }
            }
            catch
            {
                // Ignore errors in removing message
            }
        }

        /// <summary>
        /// Handles the form load event
        /// </summary>
        private async void ChatAssistant_Load(object sender, EventArgs e)
        {
            // Apply theme
            ThemeManager.ApplyThemeTo(this);
            
            // Display welcome message
            AppendMessage("Assistant: Welcome to the ArduPilot AI Assistant! How can I help you today?", Color.Black);
            
            // Check AI backend connection
            await CheckBackendConnectionAsync();
        }

        /// <summary>
        /// Load available models from Ollama
        /// </summary>
        private async void LoadAvailableModels()
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = await client.GetAsync("http://localhost:11434/api/tags");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var data = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
                        
                        modelComboBox.Items.Clear();
                        
                        foreach (var model in data.models)
                        {
                            string modelName = model.name.ToString();
                            modelComboBox.Items.Add(modelName);
                        }
                        
                        if (modelComboBox.Items.Count > 0)
                        {
                            modelComboBox.SelectedIndex = 0;
                        }
                        
                        UpdateConnectionStatus(true);
                    }
                    else
                    {
                        // Fallback to default models
                        modelComboBox.Items.AddRange(new object[] { "qwen2.5:3b", "qwen2.5:7b", "qwen2.5:14b" });
                        modelComboBox.SelectedIndex = 0;
                        UpdateConnectionStatus(false);
                    }
                }
            }
            catch
            {
                // Fallback to default models if Ollama is not running
                modelComboBox.Items.AddRange(new object[] { "qwen2.5:3b", "qwen2.5:7b", "qwen2.5:14b" });
                modelComboBox.SelectedIndex = 0;
                UpdateConnectionStatus(false);
            }
        }

        /// <summary>
        /// Update connection status indicator
        /// </summary>
        private void UpdateConnectionStatus(bool connected)
        {
            isConnected = connected;
            
            if (connected)
            {
                connectionButton.ForeColor = Color.FromArgb(0, 200, 0); // Green
                connectionButton.Text = "🔌";
            }
            else
            {
                connectionButton.ForeColor = Color.Red;
                connectionButton.Text = "🔌";
            }
        }

        /// <summary>
        /// Handle connection button click
        /// </summary>
        private async void connectionButton_Click(object sender, EventArgs e)
        {
            if (isConnected)
            {
                // Disconnect - just update status
                UpdateConnectionStatus(false);
                AppendMessage("[System: Disconnected from AI Backend]", Color.Gray);
            }
            else
            {
                // Try to connect
                AppendMessage("[System: Connecting to AI Backend...]", Color.Gray);
                
                try
                {
                    bool isHealthy = await aiService.CheckHealthAsync();
                    
                    if (isHealthy)
                    {
                        LoadAvailableModels();
                        AppendMessage("[System: AI Backend connected ✓]", Color.Green);
                    }
                    else
                    {
                        UpdateConnectionStatus(false);
                        AppendMessage("[System: AI Backend not available. Please start the backend server.]", Color.OrangeRed);
                    }
                }
                catch
                {
                    UpdateConnectionStatus(false);
                    AppendMessage("[System: Could not connect to AI Backend]", Color.Red);
                }
            }
        }

        /// <summary>
        /// Check if AI backend is connected and display status
        /// </summary>
        private async Task CheckBackendConnectionAsync()
        {
            try
            {
                bool isHealthy = await aiService.CheckHealthAsync();
                
                if (isHealthy)
                {
                    UpdateConnectionStatus(true);
                    AppendMessage("[System: AI Backend connected ✓]", Color.Green);
                }
                else
                {
                    UpdateConnectionStatus(false);
                    AppendMessage("[System: AI Backend not available. Please start the backend server.]", Color.OrangeRed);
                }
            }
            catch (Exception ex)
            {
                UpdateConnectionStatus(false);
                AppendMessage("[System: Could not check AI backend status]", Color.Gray);
            }
        }
    }
}
