using System;
using System.Drawing;
using System.Windows.Forms;
using log4net;

namespace MissionPlanner
{
    public partial class MainV2
    {
        private void InitializeChatSidebar()
        {
            // Configure the sidebar panel
            chatSidebarPanel.BackColor = Color.FromArgb(250, 250, 250);
            chatSidebarPanel.Dock = DockStyle.Right;
            chatSidebarPanel.Width = 0; // Start collapsed
            chatSidebarPanel.Visible = false;
            chatSidebarPanel.BringToFront();
        }

        private void ToggleChatSidebar()
        {
            try
            {
                if (chatSidebarPanel.Width == 0 || !chatSidebarPanel.Visible)
                {
                    // Show sidebar with animation
                    chatSidebarPanel.Visible = true;
                    chatSidebarPanel.BringToFront();
                    
                    // Create chat assistant control if not exists
                    if (chatSidebarPanel.Controls.Count == 0)
                    {
                        var chatControl = new GCSViews.ChatAssistant();
                        chatControl.Dock = DockStyle.Fill;
                        chatSidebarPanel.Controls.Add(chatControl);
                    }
                    
                    // Animate width from 0 to 400
                    var timer = new System.Windows.Forms.Timer();
                    timer.Interval = 10;
                    int targetWidth = 400;
                    timer.Tick += (s, args) =>
                    {
                        if (chatSidebarPanel.Width < targetWidth)
                        {
                            chatSidebarPanel.Width += 20;
                        }
                        else
                        {
                            chatSidebarPanel.Width = targetWidth;
                            timer.Stop();
                            timer.Dispose();
                        }
                    };
                    timer.Start();
                }
                else
                {
                    // Hide sidebar with animation
                    var timer = new System.Windows.Forms.Timer();
                    timer.Interval = 10;
                    timer.Tick += (s, args) =>
                    {
                        if (chatSidebarPanel.Width > 0)
                        {
                            chatSidebarPanel.Width -= 20;
                        }
                        else
                        {
                            chatSidebarPanel.Width = 0;
                            chatSidebarPanel.Visible = false;
                            timer.Stop();
                            timer.Dispose();
                        }
                    };
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                CustomMessageBox.Show("Error toggling AI Assistant: " + ex.Message);
            }
        }
    }
}
