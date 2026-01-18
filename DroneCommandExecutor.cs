using System;
using System.Threading.Tasks;
using MissionPlanner.Comms;

namespace MissionPlanner
{
    /// <summary>
    /// Executes drone commands via Mission Planner's MAVLink connection
    /// </summary>
    public class DroneCommandExecutor
    {
        private MAVLinkInterface mavlink;

        /// <summary>
        /// Initialize the command executor with Mission Planner's MAVLink interface
        /// </summary>
        /// <param name="comPort">Mission Planner's MAVLink interface (MainV2.comPort)</param>
        public DroneCommandExecutor(MAVLinkInterface comPort)
        {
            mavlink = comPort;
        }

        /// <summary>
        /// Execute a drone command
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <returns>Result message</returns>
        public async Task<string> ExecuteCommand(DroneCommand command)
        {
            // Check if connected to drone
            if (mavlink == null || !mavlink.BaseStream.IsOpen)
            {
                return "[Error: Not connected to drone]";
            }

            if (command == null || string.IsNullOrEmpty(command.Type))
            {
                return "[Error: Invalid command]";
            }

            try
            {
                switch (command.Type.ToUpper())
                {
                    case "ARM":
                        return ArmDisarm(true);

                    case "DISARM":
                        return ArmDisarm(false);

                    case "TAKEOFF":
                        if (command.Parameters.ContainsKey("altitude"))
                        {
                            double altitude = Convert.ToDouble(command.Parameters["altitude"]);
                            return Takeoff(altitude);
                        }
                        return "[Error: TAKEOFF requires altitude parameter]";

                    case "LAND":
                        return Land();

                    case "RTL":
                        return ReturnToLaunch();

                    case "CHANGE_MODE":
                        if (command.Parameters.ContainsKey("mode"))
                        {
                            string mode = command.Parameters["mode"].ToString();
                            return ChangeMode(mode);
                        }
                        return "[Error: CHANGE_MODE requires mode parameter]";

                    case "GOTO":
                        if (command.Parameters.ContainsKey("latitude") && command.Parameters.ContainsKey("longitude"))
                        {
                            double lat = Convert.ToDouble(command.Parameters["latitude"]);
                            double lon = Convert.ToDouble(command.Parameters["longitude"]);
                            return GoTo(lat, lon);
                        }
                        return "[Error: GOTO requires latitude and longitude parameters]";

                    default:
                        return $"[Error: Unknown command type: {command.Type}]";
                }
            }
            catch (Exception ex)
            {
                return $"[Error executing {command.Type}: {ex.Message}]";
            }
        }

        /// <summary>
        /// Arm or disarm the vehicle
        /// </summary>
        private string ArmDisarm(bool arm)
        {
            try
            {
                if (mavlink.doARM(mavlink.MAV.sysid, mavlink.MAV.compid, arm))
                {
                    return arm ? "✓ Armed" : "✓ Disarmed";
                }
                else
                {
                    return arm ? "[Error: Failed to arm]" : "[Error: Failed to disarm]";
                }
            }
            catch (Exception ex)
            {
                return $"[Error: {ex.Message}]";
            }
        }

        /// <summary>
        /// Takeoff to specified altitude
        /// </summary>
        private string Takeoff(double altitude)
        {
            try
            {
                // Set mode to GUIDED first (required for takeoff)
                mavlink.setMode(mavlink.MAV.sysid, mavlink.MAV.compid, "GUIDED");
                
                // Small delay to ensure mode change
                System.Threading.Thread.Sleep(500);
                
                // Send takeoff command using the correct signature
                mavlink.doCommand(
                    (byte)mavlink.MAV.sysid,
                    (byte)mavlink.MAV.compid,
                    MAVLink.MAV_CMD.TAKEOFF,
                    0, 0, 0, 0,  // param1-4 (pitch, empty, empty, yaw)
                    0, 0,         // param5-6 (lat, lon - use current)
                    (float)altitude  // param7 (altitude)
                );

                return $"✓ Taking off to {altitude}m";
            }
            catch (Exception ex)
            {
                return $"[Error: {ex.Message}]";
            }
        }

        /// <summary>
        /// Land the vehicle
        /// </summary>
        private string Land()
        {
            try
            {
                // Set mode to LAND
                mavlink.setMode(mavlink.MAV.sysid, mavlink.MAV.compid, "LAND");
                return "✓ Landing";
            }
            catch (Exception ex)
            {
                return $"[Error: {ex.Message}]";
            }
        }

        /// <summary>
        /// Return to launch
        /// </summary>
        private string ReturnToLaunch()
        {
            try
            {
                // Set mode to RTL
                mavlink.setMode(mavlink.MAV.sysid, mavlink.MAV.compid, "RTL");
                return "✓ Returning to launch";
            }
            catch (Exception ex)
            {
                return $"[Error: {ex.Message}]";
            }
        }

        /// <summary>
        /// Change flight mode
        /// </summary>
        private string ChangeMode(string mode)
        {
            try
            {
                // Set the requested mode
                mavlink.setMode(mavlink.MAV.sysid, mavlink.MAV.compid, mode.ToUpper());
                return $"✓ Mode changed to {mode.ToUpper()}";
            }
            catch (Exception ex)
            {
                return $"[Error: {ex.Message}]";
            }
        }

        /// <summary>
        /// Go to specified coordinates
        /// </summary>
        private string GoTo(double latitude, double longitude)
        {
            try
            {
                // Send GOTO command
                mavlink.doCommand(
                    mavlink.MAV.sysid,
                    mavlink.MAV.compid,
                    MAVLink.MAV_CMD.DO_REPOSITION,
                    -1,  // ground speed (-1 = use default)
                    (int)MAVLink.MAV_DO_REPOSITION_FLAGS.CHANGE_MODE,  // flags
                    0, 0,  // reserved
                    (float)latitude,
                    (float)longitude,
                    0  // altitude (0 = maintain current)
                );

                return $"✓ Flying to {latitude}, {longitude}";
            }
            catch (Exception ex)
            {
                return $"[Error: {ex.Message}]";
            }
        }
    }
}
