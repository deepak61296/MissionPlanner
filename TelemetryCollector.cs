using System;
using System.Collections.Generic;
using MissionPlanner.Comms;

namespace MissionPlanner
{
    /// <summary>
    /// Collects telemetry data from Mission Planner for AI backend
    /// </summary>
    public class TelemetryCollector
    {
        private MAVLinkInterface mavlink;

        public TelemetryCollector(MAVLinkInterface comPort)
        {
            mavlink = comPort;
        }

        /// <summary>
        /// Collect all available telemetry data
        /// </summary>
        public Dictionary<string, object> CollectAll()
        {
            var telemetry = new Dictionary<string, object>();

            try
            {
                if (mavlink != null && mavlink.BaseStream != null && mavlink.BaseStream.IsOpen && mavlink.MAV != null && mavlink.MAV.cs != null)
                {
                    var cs = mavlink.MAV.cs;
                    
                    telemetry["battery"] = GetBatteryData(cs);
                    telemetry["gps"] = GetGPSData(cs);
                    telemetry["attitude"] = GetAttitudeData(cs);
                    telemetry["speed"] = GetSpeedData(cs);
                    telemetry["status"] = GetFlightStatus(cs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error collecting telemetry: {ex.Message}");
            }

            return telemetry;
        }

        private Dictionary<string, object> GetBatteryData(CurrentState cs)
        {
            return new Dictionary<string, object>
            {
                ["voltage"] = cs.battery_voltage,
                ["current"] = cs.current,
                ["remaining"] = cs.battery_remaining
            };
        }

        private Dictionary<string, object> GetGPSData(CurrentState cs)
        {
            return new Dictionary<string, object>
            {
                ["latitude"] = cs.lat,
                ["longitude"] = cs.lng,
                ["altitude"] = cs.alt,
                ["satellites"] = cs.satcount,
                ["fix_type"] = cs.gpsstatus.ToString()
            };
        }

        private Dictionary<string, object> GetAttitudeData(CurrentState cs)
        {
            return new Dictionary<string, object>
            {
                ["roll"] = cs.roll,
                ["pitch"] = cs.pitch,
                ["yaw"] = cs.yaw
            };
        }

        private Dictionary<string, object> GetSpeedData(CurrentState cs)
        {
            return new Dictionary<string, object>
            {
                ["ground_speed"] = cs.groundspeed,
                ["air_speed"] = cs.airspeed,
                ["climb_rate"] = cs.climbrate
            };
        }

        private Dictionary<string, object> GetFlightStatus(CurrentState cs)
        {
            return new Dictionary<string, object>
            {
                ["mode"] = cs.mode,
                ["armed"] = cs.armed
            };
        }
    }
}
