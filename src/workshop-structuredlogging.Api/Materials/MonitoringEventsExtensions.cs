using Coolblue.Utilities.MonitoringEvents;

namespace workshop_structuredlogging.Api.Materials
{
    public static class MonitoringEventsExtensions
    {
        public static void LogProcessSubstep(this MonitoringEvents MonitoringEvents, string processName,
            string processId, string newState, string substepName)
        {
            MonitoringEvents.Logger.Information("Process [{name:id}] has entered state {state} for substep: {substep}.",
                processName, processId, newState, substepName);
        }
    }
}