using DeadworksManaged.Api;

namespace ExampleTimerPlugin;

public class ExampleTimerPlugin : DeadworksPluginBase
{
    public override string Name => "ExampleTimer";

    private IHandle? _heartbeat;

    public override void OnLoad(bool isReload)
    {
        Console.WriteLine($"[ExampleTimer] {(isReload ? "Reloaded" : "Loaded")}!");

        // Once: fire a single callback after 3 seconds
        Timer.Once(3.Seconds(), () =>
            Console.WriteLine("[ExampleTimer] 3 seconds have passed since load!"));

        // Every: fire repeatedly every 64 ticks, save handle to cancel later
        _heartbeat = Timer.Every(64.Ticks(), () =>
            Console.WriteLine("[ExampleTimer] Heartbeat (every 64 ticks)"));

        // Sequence: count to 5 with 500ms gaps, using Run, ElapsedTicks, Wait, and Done
        Timer.Sequence(step =>
        {
            Console.WriteLine($"[ExampleTimer] Sequence step {step.Run}/5 (elapsed: {step.ElapsedTicks} ticks)");
            return step.Run < 5 ? step.Wait(500.Milliseconds()) : step.Done();
        });

        // NextTick: deferred to the very next frame
        Timer.NextTick(() =>
            Console.WriteLine("[ExampleTimer] NextTick executed!"));

        // IsFinished: schedule a timer and check its status
        var probe = Timer.Once(1.Seconds(), () =>
            Console.WriteLine("[ExampleTimer] Probe fired"));
        Console.WriteLine($"[ExampleTimer] Probe finished immediately? {probe.IsFinished}");
    }

    public void OnStartupServer()
    {
        Console.WriteLine("[ExampleTimer] Server starting - scheduling round timers");

        Timer.Every(60.Seconds(), () =>
            Console.WriteLine("[ExampleTimer] 60-second announcement"));

        Timer.Once(30.Seconds(), () =>
            Console.WriteLine("[ExampleTimer] 30 seconds into the round"));
    }

    public override void OnUnload()
    {
        // Cancel: stop the heartbeat (all others are cleaned up automatically by the framework)
        _heartbeat?.Cancel();
        Console.WriteLine($"[ExampleTimer] Heartbeat cancelled, IsFinished={_heartbeat?.IsFinished}");
        Console.WriteLine("[ExampleTimer] Unloaded");
    }
}
