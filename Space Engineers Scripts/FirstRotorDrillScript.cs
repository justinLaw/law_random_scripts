IMyMotorAdvancedStator right_rotor;
IMyMotorAdvancedStator left_rotor;
const string right_rotor_name = "Right Rotor";
const string left_rotor_name = "Left Rotor";
const float ROTOR_FORWARD_ANGLE = 270f;
const float ROTOR_DOWN_ANGLE = 0f;

string delimiter = ";";
string stateDataString = "";

Dictionary<string, object> stateData = new Dictionary<string, object>();


public Program()
{
    //Runtime.UpdateFrequency = UpdateFrequency.Update10; // Set the update frequency of the script
    right_rotor = GridTerminalSystem.GetBlockWithName(right_rotor_name) as IMyMotorAdvancedStator;
    left_rotor = GridTerminalSystem.GetBlockWithName(left_rotor_name) as IMyMotorAdvancedStator;
    // Replace "RotorName" with the name of your Advanced Rotor block

    // Load the previously saved state from Storage
    if (Storage != null && Storage.Length > 0)
    {
        stateDataString = Storage;
        DeserializeStateData();
    }

    // Initialize or set default values for your state variables
    if (!stateData.ContainsKey("DRILL_STATE"))
    {
        stateData["DRILL_STATE"] = "down";
    }

}

public void Main(string argument, UpdateType updateSource)
{

    if ((string)stateData["DRILL_STATE"] == "down")
    {
        RotateRotorForward (right_rotor, true);
        RotateRotorForward (left_rotor, false);
        stateData["DRILL_STATE"] = "forward";
    }
    else
    {
        RotateRotorDown (right_rotor, true);
        RotateRotorDown (left_rotor, false);
        stateData["DRILL_STATE"] = "down";
    }

    // Save the updated state to Storage
    SerializeStateData();
    Storage = stateDataString;
}

void RotateRotorForward (IMyMotorAdvancedStator rotor, bool is_right)
{
    if (is_right) {
        rotor.RotorLock = true;
        rotor.TargetVelocityRPM = (float)0;
        rotor.UpperLimitDeg = 1000000f;
        rotor.LowerLimitDeg = ROTOR_FORWARD_ANGLE;
        rotor.TargetVelocityRPM = -1f;
        rotor.RotorLock = false;
    }
    else {
        rotor.RotorLock = true;
        rotor.TargetVelocityRPM = (float)0;
        rotor.LowerLimitDeg = -1000000f;
        rotor.UpperLimitDeg = (ROTOR_FORWARD_ANGLE * -1);
        rotor.TargetVelocityRPM = 1f;
        rotor.RotorLock = false;
    }

}


void RotateRotorDown (IMyMotorAdvancedStator rotor, bool is_right)
{

    if (is_right) {
        rotor.RotorLock = true;
        rotor.TargetVelocityRPM = (float)0;
        rotor.LowerLimitDeg = -1000000f;
        rotor.UpperLimitDeg = ROTOR_DOWN_ANGLE;
        rotor.TargetVelocityRPM = 1f;
        rotor.RotorLock = false;
    }
    else {
        rotor.RotorLock = true;
        rotor.TargetVelocityRPM = (float)0;
        rotor.UpperLimitDeg = 1000000f;
        rotor.LowerLimitDeg = ROTOR_DOWN_ANGLE;
        rotor.TargetVelocityRPM = -1f;
        rotor.RotorLock = false;
    }


}




void SerializeStateData()
{
    List<string> serializedData = new List<string>();
    foreach (var kvp in stateData)
    {
        string variable = $"{kvp.Key}:{kvp.Value.ToString()}";
        serializedData.Add(variable);
    }
    stateDataString = string.Join(delimiter, serializedData);
}

void DeserializeStateData()
{
    stateData.Clear();
    string[] variables = stateDataString.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
    foreach (string variable in variables)
    {
        string[] parts = variable.Split(':');
        if (parts.Length == 2)
        {
            string key = parts[0];
            string valueString = parts[1];
            // You might need to handle specific data types if necessary
            // For example, convert valueString to an int, float, bool, etc.
            stateData[key] = valueString;
        }
    }
}
