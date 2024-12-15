public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

// Settings
private const float BlinkInterval = 3.0f;
private readonly Color AlertColor = new Color(255, 165, 0); // Orange
private readonly Color NormalColor = new Color(0, 224, 255); // Cyan
private const float DepressurizeWaitTime = 4f; // Adjust based on hangar size
private const float DoorOperationWaitTime = 5f; // Adjust based on door speed

private string currentCommand = "";
private float timer = 0f;
private bool waiting = false;

public void Main(string argument, UpdateType updateSource)
{
    if ((updateSource & UpdateType.Trigger) != 0 || (updateSource & UpdateType.Terminal) != 0)
    {
        currentCommand = argument.ToLower();
        timer = 0f;
        waiting = false;
    }

    if (!string.IsNullOrEmpty(currentCommand))
    {
        switch (currentCommand)
        {
            case "open left":
                ExecuteHangarSequence("Hangar Door Left");
                break;

            case "open right":
                ExecuteHangarSequence("Hangar Door Right");
                break;

            case "pressurize hangar":
                ExecutePressurizeSequence();
                break;
            case "done depressurizing":
                ExecuteHangarSequence("");
                break;
            case "done pressurizing":
                ExecutePressurizeSequence();
                break;
            default:
                Echo($"Unknown command: {currentCommand}");
                currentCommand = "";
                break;
        }
    }
}

private void ExecuteHangarSequence(string hangarDoorGroupName)
{
    if (!waiting && currentCommand != "" && currentCommand != "done depressurizing")
    {
        Echo("Close Interior Doors: Depressurzie Vents: Alert Light Color");
        CloseInteriorDoors();
        SetVentsDepressurize(true);
        SetLights(AlertColor, true);
        waiting = true;
        timer = 0f;
    }

    timer += (float)Runtime.TimeSinceLastRun.TotalSeconds;

    if (timer >= DepressurizeWaitTime && waiting && currentCommand != "done")
    {
        Echo("Opening Hangar Door " + hangarDoorGroupName);
        OpenHangarDoors(hangarDoorGroupName);
        waiting = false;
        timer = 0f;
        currentCommand = "done depressurizing";
    }
    else if (timer >= DepressurizeWaitTime + DoorOperationWaitTime && !waiting)
    {
        Echo("Setting Lights Normal");
        SetLights(NormalColor, false);
        currentCommand = "";
    }
}

private void ExecutePressurizeSequence()
{
    if (!waiting && currentCommand != "" && currentCommand != "done pressurizing")
    {
        Echo("Close Hangar Doors: Alert Lights");
        CloseHangarDoors("Hangar Door Left");
        CloseHangarDoors("Hangar Door Right");
        SetLights(AlertColor, true);
        waiting = true;
        timer = 0f;
    }

    timer += (float)Runtime.TimeSinceLastRun.TotalSeconds;

    if (timer >= DoorOperationWaitTime && waiting)
    {
        Echo("Vents depressurizing");
        SetVentsDepressurize(false);
        waiting = false;
        timer = 0f;
        currentCommand = "done pressurizing";
    }
    else if (AllVentsPressurized() && !waiting)
    {
        Echo("Setting Lights Normal");
        SetLights(NormalColor, false);
        currentCommand = "";
    }
}

private void CloseInteriorDoors()
{
    var doors = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlockGroupWithName("All Interior Hangar Doors")?.GetBlocks(doors);

    if (doors == null) return;
    foreach (IMyDoor door in doors)
    {
        door.CloseDoor();
    }
}

private void OpenHangarDoors(string groupName)
{
    var doors = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlockGroupWithName(groupName)?.GetBlocks(doors);
    if (doors == null) return;
    foreach (IMyAirtightHangarDoor door in doors)
    {
        door.OpenDoor();
    }
}

private void CloseHangarDoors(string groupName)
{
    var doors = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlockGroupWithName(groupName)?.GetBlocks(doors);
    if (doors == null) return;
    foreach (IMyAirtightHangarDoor door in doors)
    {
        door.CloseDoor();
    }
}

private void SetVentsDepressurize(bool depressurize)
{
    var vents = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlockGroupWithName("All Hangar Vents")?.GetBlocks(vents);
    if (vents == null) return;
    foreach (IMyAirVent vent in vents)
    {
        vent.Depressurize = depressurize;
    }
}

private bool AllVentsPressurized()
{
    var vents = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlockGroupWithName("All Hangar Vents")?.GetBlocks(vents);
    if (vents == null) return false;
    foreach (IMyAirVent vent in vents)
    {
        if (vent.Status != VentStatus.Pressurized)
        {
            return false;
        }
    }
    return true;
}

private void SetLights(Color color, bool blink)
{
    var lights = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlockGroupWithName("All Hangar Lights")?.GetBlocks(lights);
    if (lights == null) return;
    foreach (IMyInteriorLight light in lights)
    {
        light.Color = color;
        light.BlinkIntervalSeconds = blink ? BlinkInterval : 0f;
        light.BlinkLength = blink ? 50f : 0f;
    }
}
