using System;
using System.Collections.Generic;

namespace MissionPlanner
{
    /// <summary>
    /// Represents a drone command parsed from AI response
    /// </summary>
    public class DroneCommand
    {
        /// <summary>
        /// Command type (ARM, DISARM, TAKEOFF, LAND, RTL, GOTO)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Command parameters (e.g., altitude for TAKEOFF)
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }

        public DroneCommand()
        {
            Parameters = new Dictionary<string, object>();
        }

        public override string ToString()
        {
            if (Parameters.Count == 0)
                return Type;
            
            var paramStr = string.Join(", ", Parameters);
            return $"{Type} ({paramStr})";
        }
    }

    /// <summary>
    /// Represents the complete AI response including optional command
    /// </summary>
    public class AIResponse
    {
        /// <summary>
        /// Whether the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The AI's text response to display to user
        /// </summary>
        public string Response { get; set; }

        /// <summary>
        /// Optional drone command extracted from response
        /// </summary>
        public DroneCommand Command { get; set; }

        /// <summary>
        /// Error message if request failed
        /// </summary>
        public string Error { get; set; }

        public AIResponse()
        {
            Success = false;
            Response = string.Empty;
            Command = null;
            Error = null;
        }
    }
}
