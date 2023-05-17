#if UNITY_EDITOR
using UnityEditor;
#endif

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

#if UNITY_EDITOR
        /// <summary>
        /// Setup the box transform for previewing the layout
        /// </summary>
        /// <param name="setupData">The setup data of the list</param>
        public static void PreviewBoxLayout(ListSetupData setupData)
        {
            var transformController = new Linear.BoxTransformController(setupData);
            var boxes = setupData.ListBoxes;
            var numOfBoxes = boxes.Count;
            var undoGroupID = Undo.GetCurrentGroup();
            for (var i = 0; i < numOfBoxes; ++i) {
                var box = boxes[i];
                var transform = box.GetTransform();
                Undo.RegisterCompleteObjectUndo(
                    transform, "Generate Boxes and Arrange");
                Undo.CollapseUndoOperations(undoGroupID);
                transformController.SetInitialLocalTransform(box, i);
            }
        }
#endif
    }
}
