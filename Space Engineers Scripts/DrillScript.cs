public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

IMyMotorStator rotor;
IMyShipDrill baseDrill;
List<IMyPistonBase> downwardPistons = new List<IMyPistonBase>();
IMyCockpit cockpit;
IMyTextSurface lcd_left;
IMyTextSurface lcd_mid_left;
IMyTextSurface lcd_mid;
IMyTextSurface lcd_mid_right;
IMyPistonBase hor_piston;
IMyPistonBase base_piston;

public void Main(string argument, UpdateType updateSource)
{
    if (updateSource == UpdateType.Update10)
    {
        if (downwardPistons.Count == 0)
        {
            GetDownwardPistons();
        }

        UpdateLCD();
    }
}

void GetDownwardPistons()
{
    List<IMyPistonBase> allPistons = new List<IMyPistonBase>();
    GridTerminalSystem.GetBlocksOfType(allPistons);

    string pistonNamePattern = "Piston - Drill Downward";
    foreach (IMyPistonBase piston in allPistons)
    {
        if (piston.CustomName.StartsWith(pistonNamePattern))
        {
            downwardPistons.Add(piston);
        }
    }
}

void UpdateLCD()
{
    StringBuilder main_piston_message = new StringBuilder();
    StringBuilder down_piston_message = new StringBuilder();
    StringBuilder drill_message = new StringBuilder();

    if(rotor == null) {
        rotor = GridTerminalSystem.GetBlockWithName("Rotor - Drill Base") as IMyMotorStator;
    }

    if (baseDrill == null)
    {
        baseDrill = GridTerminalSystem.GetBlockWithName("Drill - Base Drill") as IMyShipDrill;
    }

    if (cockpit == null)
    {
        cockpit = GridTerminalSystem.GetBlockWithName("Cockpit - Drill") as IMyCockpit;
        lcd_left = cockpit.GetSurface(0);
        lcd_mid_left = cockpit.GetSurface(1);
        lcd_mid = cockpit.GetSurface(2);
        lcd_mid_right = cockpit.GetSurface(3);
    }

    if (base_piston == null)
    {
        base_piston = GridTerminalSystem.GetBlockWithName("Piston - Drill Base") as IMyPistonBase;
    }

    if (hor_piston == null)
    {
        hor_piston = GridTerminalSystem.GetBlockWithName("Piston - Drill Horizontal") as IMyPistonBase;
    }

    if (base_piston != null)
    {
        main_piston_message.Append(GetPistonStatusMessage(base_piston, "Base Piston"));
    }

    if (hor_piston != null)
    {
        main_piston_message.Append(GetPistonStatusMessage(hor_piston, "Horizontal Piston"));
    }

    if (downwardPistons.Count > 0)
    {
        // Sort the downward pistons based on their CustomName
        downwardPistons.Sort((piston1, piston2) =>
        {
            string name1 = piston1.CustomName;
            string name2 = piston2.CustomName;

            // Extract the numeric part of the CustomName (e.g., "Piston - Drill Downward 1" => "1")
            string numberStr1 = name1.Substring(name1.LastIndexOf(' ') + 1);
            string numberStr2 = name2.Substring(name2.LastIndexOf(' ') + 1);

            // Parse the numeric part as integers and compare
            int number1 = int.Parse(numberStr1);
            int number2 = int.Parse(numberStr2);
            return number1.CompareTo(number2);
        });
        int index = 1;
        foreach (IMyPistonBase piston in downwardPistons)
        {
            down_piston_message.Append(GetPistonStatusMessage(piston, $"Down Piston {index}"));
            index++;
        }
    }
    if (baseDrill != null) {
        drill_message.Append(GetDrillStatusMessage(baseDrill, $"Drill 1"));
    }

    if (rotor != null)
    {
        drill_message.Append(GetRotorStatusMessage(rotor, $"Rotor"));
    }



    SetDefaultLCDSettings(lcd_mid_left);
    lcd_mid_left.WriteText(main_piston_message.ToString());
    SetDefaultLCDSettings(lcd_mid_right, 1f);
    lcd_mid_right.WriteText(down_piston_message.ToString());
    SetDefaultLCDSettings(lcd_mid);
    lcd_mid.WriteText(drill_message.ToString());
}
void SetDefaultLCDSettings (IMyTextSurface lcd, float font_size = 1.5f)
{
    IMyTextPanel text_panel = lcd as IMyTextPanel;
    Color default_panel_bg_color = new Color(0, 72, 156);
    Color default_panel_txt_color = new Color(0, 0, 255);
    Color default_bg_color = new Color(214, 137, 0);
    Color default_txt_color = new Color(0, 0, 133);

    if (text_panel != null) {
        lcd.BackgroundColor = default_panel_bg_color;
        lcd.FontColor = default_panel_txt_color;
        lcd.FontSize = 3f;
    } else {
        lcd.BackgroundColor = default_bg_color;
        lcd.FontColor = default_txt_color;
        lcd.FontSize = font_size;
    }

}

string GetPistonStatusMessage (IMyPistonBase piston, string pistonName)
{
    // Get the current position of the piston
    float currentPosition = piston.CurrentPosition;

    int percentage = (int)Math.Round((currentPosition / piston.MaxLimit) * 100f);

    // Get the current velocity of the piston
    float currentVelocity = piston.Velocity;

    // Check if the piston is at its minimum or maximum limit
    bool isAtMinLimit = piston.CurrentPosition <= piston.MinLimit + 0.01f; // Add a small buffer for precision
    bool isAtMaxLimit = piston.CurrentPosition >= piston.MaxLimit - 0.01f; // Add a small buffer for precision

    // Set the velocity to 0 if it's not moving or if it's at its limit
    if (!piston.Enabled || isAtMinLimit || isAtMaxLimit)
    {
        currentVelocity = 0f;
    }

    // Build the progress bar string
    StringBuilder progressBar = new StringBuilder();
    int progressBarLength = 30;
    int filledCharacters = progressBarLength * percentage / 100;
    progressBar.Append("[");
    for (int i = 0; i < progressBarLength; i++)
    {
        if (i < filledCharacters)
        {
            progressBar.Append("|");
        }
        else
        {
            progressBar.Append(" ");
        }
    }
    progressBar.Append($"] {percentage}%");

    StringBuilder statusMessage = new StringBuilder();
    statusMessage.Append($"{pistonName}:\n");
    statusMessage.Append($"     {progressBar.ToString()}\n");
    statusMessage.Append($"     Current Velocity: {currentVelocity.ToString("F2")}m\n");
    return statusMessage.ToString();
}

string GetDrillStatusMessage (IMyShipDrill drill, string drillName)
{
    // Get the drill status
    string drillStatus = baseDrill.Enabled ? "ON" : "OFF";

    // Get the current fill percentage of the drill
    float fillPercentage = baseDrill.GetInventory(0).CurrentVolume.RawValue / (float)baseDrill.GetInventory(0).MaxVolume.RawValue;
    int percentage = (int)Math.Round(fillPercentage * 100f);

    // Build the drill progress bar string
    int progressBarLength = 30;
    int filledCharacters = progressBarLength * percentage / 100;
    StringBuilder progressBar = new StringBuilder("[");
    for (int i = 0; i < progressBarLength; i++)
    {
        if (i < filledCharacters)
        {
            progressBar.Append("|");
        }
        else
        {
            progressBar.Append(" ");
        }
    }
    progressBar.Append($"] {percentage}%");

    StringBuilder drill_message = new StringBuilder();
    drill_message.Append($"{drillName}:\n");
    drill_message.Append($"     {progressBar}\n");
    drill_message.Append($"     Status: {drillStatus}\n");
    return drill_message.ToString();
}

string GetRotorStatusMessage(IMyMotorStator rotor, string rotorName)
{
    // Get the current velocity of the rotor
    float currentVelocity = rotor.TargetVelocityRPM;


    // Set the velocity to 0 if the rotor is not moving or if it's locked
    if (!rotor.Enabled || rotor.IsLocked)
    {
        currentVelocity = 0f;
    }

    StringBuilder statusMessage = new StringBuilder();
    statusMessage.Append($"{rotorName}:\n");

    // Display status (locked/unlocked)
    statusMessage.Append($"     Status: {(rotor.IsLocked ? "Locked" : "Unlocked")}\n");
    statusMessage.Append($"     Velocity: {currentVelocity.ToString("F2")} RPM\n");
    return statusMessage.ToString();
}
