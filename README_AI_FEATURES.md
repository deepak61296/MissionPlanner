# AI Chat Features - Mission Planner

## What is This?

AI-powered chat assistant to control your drone with natural language and get real-time telemetry information.

> **⚠️ WARNING:** This software comes with **absolutely no warranty** under GPL-3.0 license. Currently tested **only on SITL**, not real hardware. Use at your own risk.

## Requirements

**This Mission Planner fork requires the AI Backend:**
- AI Backend: https://github.com/deepak61296/ArduPilot-AI-Backend

Both must be installed and running together.

## Quick Setup

### 1. Install AI Backend

Follow the setup guide: https://github.com/deepak61296/ArduPilot-AI-Backend#quick-setup

Make sure the backend is running on `http://localhost:5000`

### 2. Open AI Chat

1. Launch Mission Planner
2. Press **Ctrl+L** (or click AI Chat button)
3. Wait for "AI Backend connected ✓" message

## Using AI Chat

### Two Modes

**Agent Mode** - Control your drone  
**Ask Mode** - Get telemetry and information

Switch modes using the dropdown in the chat window.

### Agent Mode - Control Drone

Give direct commands to control your drone:

```
"arm the drone"
"takeoff to 15 meters"
"move north 20 meters"
"increase altitude by 10m"
"change mode to loiter"
"land"
"return to launch"
```

**Safety Features:**
- Commands only execute in Agent mode
- Indirect requests ask for confirmation
- Safety limits enforced (max altitude, distance)

### Ask Mode - Get Telemetry

Ask about your drone's current status:

```
"what's my battery status?"
"what is my current altitude?"
"show me yaw heading"
"what's my roll and pitch?"
"where am I?"
"what mode am I in?"
```

The AI reads real-time telemetry and responds with current values.

## Supported Commands

### Flight Control
- **ARM/DISARM:** "arm the drone", "disarm", "kill motors"
- **TAKEOFF:** "takeoff to X meters"
- **LAND:** "land the drone", "land now"
- **RTL:** "return to launch", "return home"

### Movement
- **Directional:** "move north/south/east/west X meters"
- **Altitude:** "increase/decrease altitude by X meters", "go up/down X meters"

### Mode Changes
- **Modes:** "change mode to GUIDED/LOITER/AUTO/RTL"

### Parameters
- **Get:** "what is WPNAV_SPEED?"
- **Set:** "set WPNAV_SPEED to 500"

## Troubleshooting

**"AI Backend not connected" message?**
1. Make sure backend is running: `scripts\start_backend.bat`
2. Check Ollama is running: `ollama serve`
3. Verify port 5000 is not blocked

**Commands not executing?**
- Make sure you're in **Agent mode** (not Ask mode)
- Use direct commands: "arm the drone" ✓
- Don't ask questions: "can you arm?" ✗

**AI gives wrong responses?**
- Check telemetry is updating in Mission Planner
- Try rephrasing your command
- Report issues: https://github.com/deepak61296/ArduPilot-AI-Backend/issues

## Current Limitations

- **Vehicle:** Copter only (Plane/Rover coming soon)
- **Testing:** SITL only, not tested on real hardware
- **OS:** Windows only
- **Accuracy:** 76.8% on comprehensive test suite

## Safety Reminders

1. **Always test in SITL first**
2. **Never use on real hardware without thorough testing**
3. **Keep manual control ready**
4. **Understand each command before using**
5. **This software has NO WARRANTY**

## Links

- **AI Backend Setup:** https://github.com/deepak61296/ArduPilot-AI-Backend
- **Report Issues:** https://github.com/deepak61296/ArduPilot-AI-Backend/issues
- **ArduPilot Docs:** https://ardupilot.org

---

**Use at your own risk. No warranty provided.**
