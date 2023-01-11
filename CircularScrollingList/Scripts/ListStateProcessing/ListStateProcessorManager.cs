namespace AirFishLab.ScrollingList.ListStateProcessing
{
    /// <summary>
    /// The component for managing the list state processors
    /// </summary>
    public static class ListStateProcessorManager
    {
        /// <summary>
        /// Get the component for processing list state
        /// </summary>
        /// <param name="setupData">The setup data of the list</param>
        /// <param name="movementProcessor">The generated movement processor</param>
        /// <param name="boxController">The generated box controller</param>
        public static void GetProcessors(
            ListSetupData setupData,
            out IListMovementProcessor movementProcessor,
            out IListBoxController boxController)
        {
            var linearMovementProcessor = new Linear.ListMovementProcessor();
            linearMovementProcessor.Initialize(setupData);
            var linearBoxController = new Linear.ListBoxController();
            linearBoxController.Initialize(setupData);

            linearMovementProcessor.SetListBoxController(linearBoxController);

            movementProcessor = linearMovementProcessor;
            boxController = linearBoxController;
        }
    }
}
