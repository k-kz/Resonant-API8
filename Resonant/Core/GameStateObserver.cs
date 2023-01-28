using System;
using Dalamud.Data;
using Dalamud.Game.ClientState;

namespace Resonant
{
    internal class GameStateObserver
    {
        ClientState ClientState { get; }
        DataManager DataManager { get; }

        // TODO: Use enum value from Lumina instead of string abbreviation
        string? CurrentJobAbbrev;

        public event EventHandler<string>? JobChangedEvent;

        internal GameStateObserver(ClientState clientState, DataManager dataManager)
        {
            ClientState = clientState;
            DataManager = dataManager;

            CurrentJobAbbrev = CurrentJob();
        }

        internal void Observe()
        {
            string? observedClassJob = CurrentJob();

            if (observedClassJob != CurrentJobAbbrev)
            {
                CurrentJobAbbrev = observedClassJob;
                JobChangedEvent?.Invoke(this, observedClassJob);
            }
        }

        private string CurrentJob() => ClientState.LocalPlayer?.ClassJob.GameData?.Abbreviation ?? "UNKNOWN";
    }
}