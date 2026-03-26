# Mission Planner with AI Chat

A special **fork** of Mission Planner featuring an integrated AI chat assistant for natural language drone control.

Press **Ctrl+L** to chat with your drone using plain English!

## 🤖 What Makes This Special?

This fork adds an AI-powered sidebar to Mission Planner that lets you:
- **Control your drone** using natural language ("take off to 10 meters")
- **Query telemetry** in plain English ("what's my battery level?")
- **Get instant responses** without memorizing commands

**Example:**
```
You: "arm the drone and take off to 15 meters"
AI: ✓ Arming motors... ✓ Taking off to 15m...

You: "how high am I flying?"
AI: Current altitude: 15.3 meters
```

## 📦 Download & Install

### Option 1: Download Release (Easiest)

**Latest Release:**  
https://github.com/deepak61296/MissionPlanner/releases/tag/ai_backend

1. Download `MissionPlanner-AI-v1.0.0.zip`
2. Extract anywhere
3. Run `MissionPlanner.exe` (no installation needed)
4. Install AI backend (see below)
5. Press **Ctrl+L** to start chatting!

### Option 2: Build from Source

```bash
git clone https://github.com/deepak61296/MissionPlanner.git
cd MissionPlanner
git checkout feature/ai-chat-assistant

# Open MissionPlanner.sln in Visual Studio
# Build → Build Solution
```

## 🔧 AI Backend Setup

The AI chat feature requires a local backend server.

**Install the backend:** https://github.com/deepak61296/ardupilot-ai-backend

Quick setup:
```bash
# Install Ollama (https://ollama.com/download)
ollama pull qwen2.5:3b

# Clone backend
git clone https://github.com/deepak61296/ardupilot-ai-backend.git
cd ardupilot-ai-backend

# Setup Python environment
conda create -n ai_backend python=3.10 -y
conda activate ai_backend
pip install -r requirements.txt

# Start backend
python -m backend.api_server
```

See the [backend README](https://github.com/deepak61296/ardupilot-ai-backend) for detailed setup instructions.

## 🚀 How to Use

1. **Start Mission Planner** (this fork)
2. **Start AI backend** (must be running at http://localhost:5000)
3. **Press Ctrl+L** to open AI Chat sidebar
4. **Connect to SITL** or simulator
5. **Chat with your drone!**

### Two Modes

**Agent Mode** (execute commands):
- "take off to 20 meters"
- "move forward 10 meters"
- "land now"

**Ask Mode** (query telemetry):
- "what's my battery?"
- "how high am I?"
- "what mode am I in?"

Switch modes using the dropdown in the AI Chat panel.

## 📁 Project Structure

Key files for AI integration:

```
MissionPlanner/
├── AIBackendService.cs        # Communicates with backend API
├── GCSViews/
│   ├── ChatAssistant.cs       # AI chat sidebar UI
│   └── FlightData.cs          # QuickView panel fix
├── MainV2.ChatSidebar.cs      # Sidebar integration
├── DroneCommandExecutor.cs    # Command execution
└── README_AI_FEATURES.md      # Detailed AI features guide
```

## 🎯 Features

- ✅ **Natural language control** - No memorizing commands
- ✅ **Real-time telemetry queries** - Ask questions anytime
- ✅ **Conversational UI** - Chat-like interface
- ✅ **Offline operation** - Works without internet
- ✅ **Safety filters** - Rejects dangerous commands
- ✅ **QuickView panel fix** - Displays telemetry properly

## 🔗 Links

- **AI Backend:** https://github.com/deepak61296/ardupilot-ai-backend
- **Latest Release:** https://github.com/deepak61296/MissionPlanner/releases/tag/ai_backend
- **Report Issues:** https://github.com/deepak61296/MissionPlanner/issues
- **Official Mission Planner:** https://github.com/ArduPilot/MissionPlanner

## 📖 Documentation

- [README_AI_FEATURES.md](README_AI_FEATURES.md) - Detailed AI features guide
- [Backend Setup](https://github.com/deepak61296/ardupilot-ai-backend) - Full backend documentation

## 🛠️ Development

This fork is based on ArduPilot's Mission Planner with added AI chat features.

**Main changes:**
- AI chat sidebar integration (Ctrl+L)
- Backend communication layer
- Command execution pipeline
- QuickView panel data binding fix

**Contributing:**
1. Fork this repo
2. Create feature branch
3. Test with SITL
4. Submit PR

## ⚠️ Important Notes

- **Testing:** This fork is currently tested on SITL only
- **Vehicle Support:** Copter only (Plane/Rover not yet supported)  
- **Experimental:** Test thoroughly in simulation before considering real flights
- **No Warranty:** Use at your own risk - this is independent development
- **License:** GPL-3.0 (inherited from ArduPilot/MissionPlanner)

Always test in simulation (SITL) before any real-world use.

For the official, production-ready Mission Planner, visit: https://github.com/ArduPilot/MissionPlanner
