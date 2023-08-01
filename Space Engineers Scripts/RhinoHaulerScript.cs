IMyCockpit cockpit;
IMyTextSurface cockpit_surface_left;
IMyTextSurface cockpit_surface_mid;
IMyTextSurface cockpit_surface_right;
IMyBlockGroup ship_blocks;
List<IMyShipConnector> all_connectors = new List<IMyShipConnector>();
IMyBlockGroup outgoing_lights;
IMyBlockGroup small_outgoing_lights;
IMyBlockGroup intake_lights;
IMyBlockGroup small_intake_lights;
StringBuilder status_message = new StringBuilder();

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

public void Main(string argument, UpdateType updateSource)
{
    GetShipBlocks("RH - All Ship Blocks");
    GetCockpitLCDS("RH - Cockpit");
    status_message.Clear();
    // Set cockpit default settings
    SetDefaultLCDSettings(cockpit_surface_left);
    SetDefaultLCDSettings(cockpit_surface_mid);
    SetDefaultLCDSettings(cockpit_surface_right, 1f);


    StringBuilder mid_message = new StringBuilder();

    mid_message
    .Append(GetCurrentCargoMessage());

    cockpit_surface_left.WriteText(GetEnergyStatusMessage());
    cockpit_surface_mid.WriteText(mid_message);
    cockpit_surface_right.WriteText(GetConnectorMessage());
    mid_message.Clear();
}

string GetProgressBarString(int percentage, int progressBarLength = 15)
{
    int filledCharacters = progressBarLength * percentage / 100;
    StringBuilder progressBar = new StringBuilder("[");
    for (int i = 0; i < progressBarLength; i++)
    {
        progressBar.Append(i < filledCharacters ? "|" : " ");
    }
    progressBar.Append($"]  {percentage}%");
    return progressBar.ToString();
}

string GetCurrentCargoMessage()
{
    long maxVolume = 0, currentVolume = 0;
    List<IMyInventoryOwner> containers = new List<IMyInventoryOwner>();
    ship_blocks.GetBlocksOfType<IMyInventoryOwner>(containers);

    if (containers.Count == 0) {
        return "No containers found";
    }

    // Loop the list of container blocks and sum their current and max volume
    for (int i = 0; i < containers.Count; i++) {
        maxVolume += ((IMyInventoryOwner)containers[i]).GetInventory(0).MaxVolume.RawValue;
        currentVolume += ((IMyInventoryOwner)containers[i]).GetInventory(0).CurrentVolume.RawValue;
    }

    int percentage = (int)Math.Round((float)((float)currentVolume / (float)maxVolume) * 100f);

    status_message.Clear();
    status_message
    .Append("Cargo:\n")
    .Append($"  {GetProgressBarString(percentage, 30)}\n");
    containers.Clear();
    return status_message.ToString();
}

string GetConnectorMessage()
{
    List<IMyShipConnector> intake_connectors = new List<IMyShipConnector>();
    List<IMyShipConnector> outgoing_connectors = new List<IMyShipConnector>();


    foreach (IMyShipConnector connector in all_connectors) {
      if (connector.CustomName.Contains("Outgoing")){
        outgoing_connectors.Add(connector);
      } else if (connector.CustomName.Contains("Intake")){
        intake_connectors.Add(connector);
      }
    }

    // Manually sort the connectors using the custom comparison function
    outgoing_connectors = SortConnectors(outgoing_connectors);
    intake_connectors = SortConnectors(intake_connectors);



    status_message.Clear();
    status_message
    .Append("Connectors:\n")
    .Append("   Out-Going:\n");
    BuildConnectorMessage(outgoing_connectors, status_message);

    status_message
    .Append("   Intake:\n");
    BuildConnectorMessage(intake_connectors, status_message);

    intake_connectors.Clear();
    outgoing_connectors.Clear();
    return status_message.ToString();
}

string GetEnergyStatusMessage()
{
    float maxVolume = 0,
      currentVolume = 0,
      currentOutput = 0,
      outputPerSecond = 0,
      remainingPowerSeconds = 0,
      currentInput = 0;
    List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();

    ship_blocks.GetBlocksOfType<IMyBatteryBlock>(batteries);
    status_message.Clear();
    if (batteries.Count == 0) {
      status_message
        .Append("Power Status:\n")
        .Append("   Stored Power:\n")
        .Append($"      No storage medium\n")
        .Append($"    Output: 0kW\n")
        .Append($"    Input: 0kW\n")
        .Append($"    Remaining Time: 0\n");
        return status_message.ToString();
    }

    // Loop the list of battery blocks and sum their current and max volume
    for (int i = 0; i < batteries.Count; i++) {
        maxVolume += batteries[i].MaxStoredPower;
        currentVolume += batteries[i].CurrentStoredPower;
        currentOutput += batteries[i].CurrentOutput;
        currentInput += batteries[i].CurrentInput;
    }

    int percentage = (int)Math.Round((float)((float)currentVolume / (float)maxVolume) * 100f);

    outputPerSecond = (currentOutput - currentInput) / 1000.0f;
    remainingPowerSeconds = ((currentVolume * 3600) / 1000) / outputPerSecond;

    long minutes = (long)Math.Round((decimal)(remainingPowerSeconds / 60));
    long hours = (long)Math.Round((decimal)((remainingPowerSeconds / 60) / 60));
    long days = (long)Math.Round((decimal)(((remainingPowerSeconds / 60) / 60) / 24));

    StringBuilder remainingString = new StringBuilder();
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


    status_message
      .Append("Power Status:\n")
      .Append("   Stored Power:\n")
      .Append($"    {GetProgressBarString(percentage, 30)}\n")
      .Append($"    Output: {Math.Round(currentOutput*100f)}kW\n")
      .Append($"    Input: {Math.Round(currentInput*100f)}kW\n")
      .Append($"    Remaining Time: {remainingString.ToString()}");

    batteries.Clear();
    return status_message.ToString();
}

void SetDefaultLCDSettings(IMyTextSurface lcd, float font_size = 1.5f)
{
    IMyTextPanel text_panel = lcd as IMyTextPanel;
    Color default_panel_bg_color = new Color(0, 72, 156);
    Color default_panel_txt_color = new Color(0, 0, 255);
    Color default_txt_color = new Color(93, 137, 0);
    Color default_bg_color = new Color(0, 0, 0);

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
        lcd.FontSize = font_size;
    }
    text_panel = null;
}

void GetShipBlocks (string group_name)
{
  List<IMyTerminalBlock> test_list = new List<IMyTerminalBlock>();

  if (ship_blocks == null) {
    ship_blocks = GridTerminalSystem.GetBlockGroupWithName(group_name) as IMyBlockGroup;
  } else {
    ship_blocks.GetBlocksOfType<IMyTerminalBlock>(test_list);
    if(test_list.Count == 0){
      ship_blocks = GridTerminalSystem.GetBlockGroupWithName(group_name) as IMyBlockGroup;
    }
    test_list.Clear();
  }

  if (ship_blocks != null && (all_connectors == null || all_connectors.Count == 0)) {
    ship_blocks.GetBlocksOfType<IMyShipConnector>(all_connectors);
  }

  if (outgoing_lights == null){
    outgoing_lights = GridTerminalSystem.GetBlockGroupWithName("Outgoing Lights") as IMyBlockGroup;
  } else {
    outgoing_lights.GetBlocksOfType<IMyTerminalBlock>(test_list);
    if(test_list.Count == 0){
      outgoing_lights = GridTerminalSystem.GetBlockGroupWithName("Outgoing Lights") as IMyBlockGroup;
    }
    test_list.Clear();
  }

  if(small_outgoing_lights == null){
    small_outgoing_lights = GridTerminalSystem.GetBlockGroupWithName("Small Outgoing Lights") as IMyBlockGroup;
  } else {
    small_outgoing_lights.GetBlocksOfType<IMyTerminalBlock>(test_list);
    if(test_list.Count == 0){
      small_outgoing_lights = GridTerminalSystem.GetBlockGroupWithName("Small Outgoing Lights") as IMyBlockGroup;
    }
    test_list.Clear();
  }

  if(intake_lights == null){
    intake_lights = GridTerminalSystem.GetBlockGroupWithName("Intake Lights") as IMyBlockGroup;
  } else {
    intake_lights.GetBlocksOfType<IMyTerminalBlock>(test_list);
    if(test_list.Count == 0){
      intake_lights = GridTerminalSystem.GetBlockGroupWithName("Intake Lights") as IMyBlockGroup;
    }
    test_list.Clear();
  }

  if(small_intake_lights == null){
    small_intake_lights = GridTerminalSystem.GetBlockGroupWithName("Small Intake Lights") as IMyBlockGroup;
  } else {
    small_intake_lights.GetBlocksOfType<IMyTerminalBlock>(test_list);
    if(test_list.Count == 0){
      small_intake_lights = GridTerminalSystem.GetBlockGroupWithName("Small Intake Lights") as IMyBlockGroup;
    }
    test_list.Clear();
  }
}

void GetCockpitLCDS (string cockpit_name)
{
    if (cockpit == null)
    {
        // Get cockpit
        cockpit = GridTerminalSystem.GetBlockWithName(cockpit_name) as IMyCockpit;
        // Get cockpit surfaces
        cockpit_surface_left = cockpit.GetSurface(0);
        cockpit_surface_mid = cockpit.GetSurface(1);
        cockpit_surface_right = cockpit.GetSurface(2);
    }
}

void BuildConnectorMessage (List<IMyShipConnector> connectors, StringBuilder message){
  foreach (IMyShipConnector conn in connectors){
    IMyBlockGroup conn_lights;
    StringBuilder con_status = new StringBuilder();
    MyShipConnectorStatus status = conn.Status;
    bool is_small = conn.CustomName.Contains("Small");
    bool is_intake = conn.CustomName.Contains("Intake");

    if(is_intake){
      if(is_small){
        conn_lights = small_intake_lights;
      } else {
        conn_lights = intake_lights;
      }
    } else {
      if (is_small){
        conn_lights = small_outgoing_lights;
      } else {
        conn_lights = outgoing_lights;
      }
    }

    Echo($"Status is {status.ToString()}");
    switch(status) {
      case MyShipConnectorStatus.Connectable:
        con_status.Append("Connection in range.");
        SetLightGroupsColor(conn_lights, Color.Orange, .5f, 50f);
      break;
      case MyShipConnectorStatus.Connected:
        con_status.Append("Connected");
        SetLightGroupsColor(conn_lights, Color.Green, 1f, 100f);
      break;
      case MyShipConnectorStatus.Unconnected:
        con_status.Append("No connection in range.");
        SetLightGroupsColor(conn_lights, Color.OrangeRed, 1f, 50f);
      break;
      default:
        con_status.Append("Unknown Status");
        SetLightGroupsColor(conn_lights, Color.Red, 1f, 100f);
      break;
    }

    message
    .Append($"        {(is_small ? "Small" : "Large")} Conn:\n")
    .Append($"            Status:{con_status.ToString()}\n");
  }
}

List<IMyShipConnector> SortConnectors(List<IMyShipConnector> connectors)
{
    // Manually sort the connectors using the custom comparison function
    connectors.Sort((connector1, connector2) =>
    {
        bool isSmall1 = connector1.CustomName.Contains("Small");
        bool isSmall2 = connector2.CustomName.Contains("Small");

        // Compare based on whether the names contain "Small" or not
        if (isSmall1 && !isSmall2)
            return 1; // connector1 comes first
        else if (!isSmall1 && isSmall2)
            return -1; // connector2 comes first

        // If both have "Small" or neither have it, compare alphabetically
        return string.Compare(connector1.CustomName, connector2.CustomName);
    });

    return connectors;
}

void SetLightGroupsColor (IMyBlockGroup light_group, Color color, float blink_interval = 0, float blink_length = .1f) {
  List<IMyLightingBlock> lights = new List<IMyLightingBlock>();
  light_group.GetBlocksOfType<IMyLightingBlock>(lights);

  foreach(IMyLightingBlock light in lights){
    light.Color = color;
    light.BlinkIntervalSeconds = blink_interval;
    light.BlinkLength = blink_length;
  }

  lights.Clear();
}
