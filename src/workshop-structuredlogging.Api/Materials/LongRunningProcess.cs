using System;
using Coolblue.Utilities.MonitoringEvents;

namespace workshop_structuredlogging.Api.Materials
{
    public interface ILongRunningProcess
    {
        void Run();
    }
    
    public class LongRunningProcess : ILongRunningProcess
    {
        public MonitoringEvents MonitoringEvents { get; set; }

        private string ProcessId { get; }
        private string ProcessName { get; }
        
        public LongRunningProcess()
        {
            ProcessId = Guid.NewGuid().ToString();
            ProcessName = "Long Running Process™";
        }

        public void Run()
        {
            MonitoringEvents.Logger.Information("Process [{processName}:{processId}] has entered state: {state}.", ProcessName, ProcessId, "started");
            
            GatherData();
            ProcessingData();
            MagicSauce();
            Cleanup();
            
            MonitoringEvents.Logger.Information("Process [{processName}:{processId}] has entered state: {state}.", ProcessName, ProcessId, "finished");
        }
        
        private void GatherData()
        {
            MonitoringEvents.Logger.Information(
                "Process [{processName}:{processId}] has entered state {state} for substep: {substep}.",
                ProcessName, ProcessId, "started", "Gathering data");

            System.Threading.Thread.Sleep(1500);

            MonitoringEvents.Logger.Information(
                "Process [{processName}:{processId}] has entered state {state} for substep: {substep}.",
                ProcessName, ProcessId, "finished", "Gathering data");
        }

        private void ProcessingData()
        {
            MonitoringEvents.Logger.Information(
                "Process [{processName}:{processId}] has entered state {state} for substep: {substep}.",
                ProcessName, ProcessId, "started", "Processing data");

            System.Threading.Thread.Sleep(1500);

            MonitoringEvents.Logger.Information(
                "Process [{processName}:{processId}] has entered state {state} for substep: {substep}.",
                ProcessName, ProcessId, "finished", "Processing data");
        }

        private void MagicSauce()
        {
            MonitoringEvents.Logger.Information(
                "Process [{processName}:{processId}] has entered state {state} for substep: {substep}.",
                ProcessName, ProcessId, "started", "Magic sauce");

            System.Threading.Thread.Sleep(1500);

            MonitoringEvents.Logger.Information(
                "Process [{processName}:{processId}] has entered state {state} for substep: {substep}.",
                ProcessName, ProcessId, "finished", "Magic sauce");
        }

        private void Cleanup()
        {
            MonitoringEvents.Logger.Information(
                "Process [{processName}:{processId}] has entered state {state} for substep: {substep}.",
                ProcessName, ProcessId, "started", "Cleanup");

            System.Threading.Thread.Sleep(1500);

            MonitoringEvents.Logger.Information(
                "Process [{processName}:{processId}] has entered state {state} for substep: {substep}.",
                ProcessName, ProcessId, "finished", "Cleanup");
        }
    }
}