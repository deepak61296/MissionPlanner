import re

# Read the file
with open(r'C:\Projects\ArduPilot-AI-Backend\ap_offline_chat_tool\api_server.py', 'r', encoding='utf-8') as f:
    content = f.read()

# Add the is_telemetry_valid function after the logger initialization
telemetry_check = '''

def is_telemetry_valid(telemetry):
    """Check if telemetry data indicates a real drone connection"""
    if not telemetry:
        return False
    
    # Check battery - if voltage is 0, no drone connected
    if "battery" in telemetry:
        voltage = telemetry["battery"].get("voltage", 0)
        if voltage > 0:
            return True
    
    # Check GPS - if satellites > 0, drone is connected
    if "gps" in telemetry:
        sats = telemetry["gps"].get("satellites", 0)
        if sats > 0:
            return True
    
    # Check status - if mode is not empty/default
    if "status" in telemetry:
        mode = telemetry["status"].get("mode", "")
        if mode and mode != "UNKNOWN" and mode != "":
            return True
    
    return False

'''

# Insert after logger.info line
content = content.replace(
    'logger.info(f"AI Backend API Server initialized")',
    'logger.info(f"AI Backend API Server initialized")' + telemetry_check
)

# Update AGENT_MODE_PROMPT with stricter command rules
old_agent_prompt = '''AGENT_MODE_PROMPT = """You are an AI assistant for ArduPilot Mission Planner with COMMAND EXECUTION capabilities.

CAPABILITIES:
- ARM/DISARM the drone
- TAKEOFF to specified altitude
- LAND the drone
- RTL (Return to Launch)
- Read all telemetry data

IMPORTANT RULES:
1. When user requests a command, acknowledge it clearly and concisely
2. Use exact command phrases:
   - "Arming the drone now."
   - "Taking off to X meters."
   - "Landing the drone."
   - "Returning to launch."
3. Be brief and direct
4. If asked about telemetry, provide the data clearly

CURRENT TELEMETRY:
{telemetry}

Respond naturally and helpfully."""'''

new_agent_prompt = '''AGENT_MODE_PROMPT = """You are an AI assistant for ArduPilot Mission Planner with COMMAND EXECUTION capabilities.

CAPABILITIES:
- ARM/DISARM the drone
- TAKEOFF to specified altitude
- LAND the drone
- RTL (Return to Launch)
- Read all telemetry data

CRITICAL SAFETY RULES:
1. ONLY execute commands when user EXPLICITLY requests them with clear intent
2. DO NOT suggest or execute commands when user asks informational questions
3. Examples of VALID command requests:
   - "arm the drone" → Execute ARM
   - "takeoff to 10 meters" → Execute TAKEOFF
   - "land now" → Execute LAND
4. Examples of INVALID (do NOT execute):
   - "what can you do?" → Just explain, DO NOT execute
   - "tell me where I am" → Just provide location, DO NOT arm
   - "are we connected?" → Just answer, DO NOT execute anything
5. When executing a command, use exact phrases:
   - "Arming the drone now."
   - "Taking off to X meters."
   - "Landing the drone."
   - "Returning to launch."
6. If user asks questions about telemetry, ONLY provide information, DO NOT execute commands

CONNECTION STATUS: {connection_status}

{telemetry_section}

Be helpful but SAFE. Only execute when explicitly asked."""'''

content = content.replace(old_agent_prompt, new_agent_prompt)

# Update ASK_MODE_PROMPT to be very clear about restrictions
old_ask_prompt = '''ASK_MODE_PROMPT = """You are an AI assistant for ArduPilot Mission Planner in READ-ONLY mode.

CAPABILITIES:
- Read battery status (voltage, current, remaining %)
- Read GPS position and altitude
- Read flight mode and armed status
- Read sensor data (attitude, speed, heading)
- Read mission progress
- Explain telemetry data

RESTRICTIONS:
- You CANNOT control the drone
- You CANNOT execute commands (ARM, TAKEOFF, LAND, etc.)
- You can ONLY read and explain data

PERSONALITY:
- Helpful and informative
- Explain technical terms simply
- Provide context for readings
- Be concise but thorough

CURRENT TELEMETRY:
{telemetry}

If user asks you to execute a command, politely explain that you're in Ask Mode (read-only) and cannot control the drone."""'''

new_ask_prompt = '''ASK_MODE_PROMPT = """You are an AI assistant for ArduPilot Mission Planner in READ-ONLY mode.

CAPABILITIES:
- Read battery status (voltage, current, remaining %)
- Read GPS position and altitude
- Read flight mode and armed status
- Read sensor data (attitude, speed, heading)
- Read mission progress
- Explain telemetry data

RESTRICTIONS:
- You CANNOT control the drone
- You CANNOT execute commands (ARM, TAKEOFF, LAND, RTL, etc.)
- You can ONLY read and explain data
- You are in ASK MODE (read-only)

IMPORTANT:
If user asks you to execute ANY command (arm, takeoff, land, change mode, etc.), respond with:
"I'm currently in Ask Mode (read-only) and cannot execute commands. To control the drone, please switch to Agent Mode using the mode selector at the bottom of the chat window."

PERSONALITY:
- Helpful and informative
- Explain technical terms simply
- Provide context for readings
- Be concise but thorough
- Be HONEST about connection status

CONNECTION STATUS: {connection_status}

{telemetry_section}

Remember: You can ONLY read data, never execute commands."""'''

content = content.replace(old_ask_prompt, new_ask_prompt)

# Update the chat endpoint to use connection detection
old_logic = '''        # Get telemetry
        telemetry = data.get('telemetry', {})
        telemetry_str = format_telemetry_for_prompt(telemetry) if telemetry else "No telemetry data available"
        
        logger.info(f"Processing message in {mode} mode: {user_message}")
        
        # Select prompt based on mode
        if mode == 'agent':
            system_prompt = AGENT_MODE_PROMPT.format(telemetry=telemetry_str)
        else:  # ask mode
            system_prompt = ASK_MODE_PROMPT.format(telemetry=telemetry_str)'''

new_logic = '''        # Get telemetry
        telemetry = data.get('telemetry', {})
        
        # Check if drone is actually connected
        is_connected = is_telemetry_valid(telemetry)
        
        if is_connected:
            connection_status = "CONNECTED to drone"
            telemetry_str = format_telemetry_for_prompt(telemetry)
            telemetry_section = f"CURRENT TELEMETRY:\\n{telemetry_str}"
        else:
            connection_status = "NOT CONNECTED to drone"
            telemetry_section = "TELEMETRY: No drone connected. All values are zero/default."
        
        logger.info(f"Processing message in {mode} mode: {user_message} (Connected: {is_connected})")
        
        # Select prompt based on mode
        if mode == 'agent':
            system_prompt = AGENT_MODE_PROMPT.format(
                connection_status=connection_status,
                telemetry_section=telemetry_section
            )
        else:  # ask mode
            system_prompt = ASK_MODE_PROMPT.format(
                connection_status=connection_status,
                telemetry_section=telemetry_section
            )'''

content = content.replace(old_logic, new_logic)

# Update command extraction to be stricter - only detect explicit commands
old_extract = '''def extract_command(ai_response: str) -> dict:
    """
    Extract drone command from AI response text
    Returns: {"type": "COMMAND_TYPE", "params": {...}} or None
    """
    response_lower = ai_response.lower()
    
    # ARM command
    if re.search(r'\\b(arm|arming)\\b', response_lower) and not re.search(r'\\bdisarm', response_lower):
        return {"type": "ARM", "params": {}}'''

new_extract = '''def extract_command(ai_response: str) -> dict:
    """
    Extract drone command from AI response text
    ONLY extracts if AI explicitly says it's executing the command
    Returns: {"type": "COMMAND_TYPE", "params": {...}} or None
    """
    response_lower = ai_response.lower()
    
    # ARM command - only if AI says "arming the drone now"
    if re.search(r'arming the drone now', response_lower) and not re.search(r'\\bdisarm', response_lower):
        return {"type": "ARM", "params": {}}'''

content = content.replace(old_extract, new_extract)

# Update TAKEOFF detection to be stricter
old_takeoff = '''    # TAKEOFF command with altitude
    takeoff_match = re.search(r'(?:takeoff|take off|taking off).*?(\\d+)\\s*(?:meters|m\\b)', response_lower)
    if takeoff_match:
        altitude = int(takeoff_match.group(1))
        return {"type": "TAKEOFF", "params": {"altitude": altitude}}'''

new_takeoff = '''    # TAKEOFF command with altitude - only if AI says "taking off to X meters"
    takeoff_match = re.search(r'taking off to (\\d+)\\s*(?:meters|m\\b)', response_lower)
    if takeoff_match:
        altitude = int(takeoff_match.group(1))
        return {"type": "TAKEOFF", "params": {"altitude": altitude}}'''

content = content.replace(old_takeoff, new_takeoff)

# Update LAND detection
old_land = '''    # LAND command
    if re.search(r'\\b(land|landing)\\b', response_lower):
        return {"type": "LAND", "params": {}}'''

new_land = '''    # LAND command - only if AI says "landing the drone"
    if re.search(r'landing the drone', response_lower):
        return {"type": "LAND", "params": {}}'''

content = content.replace(old_land, new_land)

# Update RTL detection
old_rtl = '''    # RTL (Return to Launch) command
    if re.search(r'\\b(rtl|return to launch|return home)\\b', response_lower):
        return {"type": "RTL", "params": {}}'''

new_rtl = '''    # RTL (Return to Launch) command - only if AI says "returning to launch"
    if re.search(r'returning to launch', response_lower):
        return {"type": "RTL", "params": {}}'''

content = content.replace(old_rtl, new_rtl)

# Update DISARM detection
old_disarm = '''    # DISARM command
    if re.search(r'\\bdisarm', response_lower):
        return {"type": "DISARM", "params": {}}'''

new_disarm = '''    # DISARM command - only if AI says "disarming the drone"
    if re.search(r'disarming the drone', response_lower):
        return {"type": "DISARM", "params": {}}'''

content = content.replace(old_disarm, new_disarm)

# Write back
with open(r'C:\Projects\ArduPilot-AI-Backend\ap_offline_chat_tool\api_server.py', 'w', encoding='utf-8') as f:
    f.write(content)

print('✅ API server updated successfully!')
print('')
print('SAFETY IMPROVEMENTS:')
print('1. ✅ Stricter command detection - only exact phrases trigger commands')
print('2. ✅ AI will NOT execute commands for informational questions')
print('3. ✅ Ask Mode clearly explains it cannot execute commands')
print('4. ✅ Connection status detection fixed')
print('')
print('Restart the API server to apply changes.')
