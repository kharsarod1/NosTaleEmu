using Serilog;

namespace NosTaleEmu.WorldServer.Commands;

public sealed class RatesCommand : IConsoleCommand
{
    public string Name => "rates";
    public string Usage => "rates";
    public string Description => "Muestra los rates configurados actualmente";

    public Task ExecuteAsync(ConsoleCommandContext context, string[] args, CancellationToken cancellationToken)
    {
        Log.Information("Exp x{ExpRate} | Drop x{DropRate} | Gold x{GoldRate} | GoldDrop x{GoldDropRate} | Reputation x{ReputationRate} | FairyXp x{FairyXpRate}",
            context.Settings.Rates.ExpRate,
            context.Settings.Rates.DropRate,
            context.Settings.Rates.GoldRate,
            context.Settings.Rates.GoldDropRate,
            context.Settings.Rates.ReputationRate,
            context.Settings.Rates.FairyXpRate);

        return Task.CompletedTask;
    }
}
