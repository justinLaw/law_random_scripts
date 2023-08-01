IMyTextPanel lcd_1;
IMyCockpit cockpit;
IMyTextSurface cockpit_surface_left;
IMyTextSurface cockpit_surface_mid;
IMyTextSurface cockpit_surface_right;



public class BatteryOutput
{
    public string PowerProgressBar { get; set; }
    public string RemainingTime { get; set; }

    public BatteryOutput(string powerProgressBar, string remainingTime)
    {
        PowerProgressBar = powerProgressBar;
        RemainingTime = remainingTime;
    }
}


public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

public void Main(string argument, UpdateType updateSource)
{
    if (updateSource == UpdateType.Update10)
    {
        GetCockpitLCDS();
    }

    // Set cockpit default settings
    SetDefaultLCDSettings(cockpit_surface_left);
    SetDefaultLCDSettings(cockpit_surface_mid);
    SetDefaultLCDSettings(cockpit_surface_right);
    SetDefaultLCDSettings(lcd_1);

    BatteryOutput power = null;

    string cargo_fill = GetCurrentCargoPercent();
    power = GetCurrentEnergyOutput();

    // Use StringBuilder to build the final combined string
    StringBuilder combo = new StringBuilder();
    combo.Append(cargo_fill).Append("\n").Append(power.PowerProgressBar).Append("\n").Append(power.RemainingTime);

    cockpit_surface_left.WriteText(combo.ToString());
    lcd_1.WriteText(combo.ToString());
}

string GetCurrentCargoPercent()
{
    long maxVolume = 0, currentVolume = 0;
    List<IMyInventoryOwner> containers = new List<IMyInventoryOwner>();
    GridTerminalSystem.GetBlocksOfType<IMyInventoryOwner>(containers);

    if (containers.Count == 0) {
        return "No containers found";
    }

    // Loop the list of container blocks and sum their current and max volume
    for (int i = 0; i < containers.Count; i++) {
        maxVolume += ((IMyInventoryOwner)containers[i]).GetInventory(0).MaxVolume.RawValue;
        currentVolume += ((IMyInventoryOwner)containers[i]).GetInventory(0).CurrentVolume.RawValue;
    }

    int percentage = (int)Math.Round((float)((float)currentVolume / (float)maxVolume) * 100f);

    // Use StringBuilder to build the progress bar string
    int progressBarLength = 15;
    int filledCharacters = progressBarLength * percentage / 100;
    StringBuilder progressBar = new StringBuilder("Cargo: [");
    for (int i = 0; i < progressBarLength; i++)
    {
        progressBar.Append(i < filledCharacters ? "||" : "_");
    }
    progressBar.Append($"]  {percentage}%");

    return progressBar.ToString();
}

BatteryOutput GetCurrentEnergyOutput()
{
    float maxVolume = 0, currentVolume = 0, currentOutput = 0, outputPerSecond = 0, remainingPowerSeconds = 0, currentInput = 0;
    List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();

    GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries);

    if (batteries.Count == 0) {
        return new BatteryOutput ("No Power Sources Present", "No Power Sources Present");
    }

    // Loop the list of battery blocks and sum their current and max volume
    for (int i = 0; i < batteries.Count; i++) {
        maxVolume += batteries[i].MaxStoredPower;
        currentVolume += batteries[i].CurrentStoredPower;
        currentOutput += batteries[i].CurrentOutput;
        currentInput += batteries[i].CurrentInput;
    }

    int percentage = (int)Math.Round((float)((float)currentVolume / (float)maxVolume) * 100f);

    // Use StringBuilder to build the progress bar string
    int progressBarLength = 15;
    int filledCharacters = progressBarLength * percentage / 100;
    StringBuilder progressBar = new StringBuilder("Power: [");
    for (int i = 0; i < progressBarLength; i++)
    {
        progressBar.Append(i < filledCharacters ? "||" : "_");
    }
    progressBar.Append($"]  {percentage}%");

    outputPerSecond = (currentOutput - currentInput) / 1000.0f;
    remainingPowerSeconds = ((currentVolume * 3600) / 1000) / outputPerSecond;

    long minutes = (long)Math.Round((decimal)(remainingPowerSeconds / 60));
    long hours = (long)Math.Round((decimal)((remainingPowerSeconds / 60) / 60));
    long days = (long)Math.Round((decimal)(((remainingPowerSeconds / 60) / 60) / 24));

    StringBuilder remainingString = new StringBuilder($"Power Remaining: ");
    if (days > 0)
    {
        remainingString.Append($"{days} days");
    }
    else if (hours > 0)
    {
        remainingString.Append($"{hours} hours");
    }
    else
    {
        remainingString.Append($"{minutes} minutes");
    }

    return new BatteryOutput(progressBar.ToString(), remainingString.ToString());
}

void SetDefaultLCDSettings(IMyTextSurface lcd)
{
    IMyTextPanel text_panel = lcd as IMyTextPanel;
    Color default_panel_bg_color = new Color(0, 72, 156);
    Color default_panel_txt_color = new Color(0, 0, 255);
    Color default_bg_color = new Color(214, 137, 0);
    Color default_txt_color = new Color(0, 0, 133);

    if (text_panel != null)
    {
        lcd.BackgroundColor = default_panel_bg_color;
        lcd.FontColor = default_panel_txt_color;
        lcd.FontSize = 3f;
    }
    else
    {
        lcd.BackgroundColor = default_bg_color;
        lcd.FontColor = default_txt_color;
        lcd.FontSize = 1.5f;
    }
}

void GetCockpitLCDS ()
{
    if (lcd_1 == null)
    {
        lcd_1 = GridTerminalSystem.GetBlockWithName("LCD - 1") as IMyTextPanel;
    }

    if (cockpit == null)
    {
        // Get cockpit
        cockpit = GridTerminalSystem.GetBlockWithName("Industrial Cockpit") as IMyCockpit;
        // Get cockpit surfaces
        cockpit_surface_left = cockpit.GetSurface(0);
        cockpit_surface_mid = cockpit.GetSurface(1);
        cockpit_surface_right = cockpit.GetSurface(2);
    }
}
