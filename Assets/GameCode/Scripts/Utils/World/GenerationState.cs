using System.Threading;

namespace GameCode.Scripts.Utils.World
{
    public class GenerationState
    {
        private int _currentStepProgress;
        public int CurrentStep { get; private set; }
        public int MaxStep { get; private set; }
        public string StepDesc { get; private set; }

        public int CurrentStepProgress => _currentStepProgress;

        public int CurrentStepMaxProgress { get; private set; }

        public bool IsFinished { get; private set; }

        public GenerationState()
        {
        }

        public GenerationState Init(int steps)
        {
            MaxStep = steps;
            return this;
        }

        public void InitStep(string stepName, int maxProgress)
        {
            StepDesc = stepName;
            CurrentStepMaxProgress = maxProgress;
        }

        public void UpdateStep(int progress)
        {
            Interlocked.Add(ref _currentStepProgress, progress);
        }

        public void CloseStep()
        {
            CurrentStep++;
            StepDesc = null;
            _currentStepProgress = 0;
            CurrentStepMaxProgress = 0;
        }

        public void Finish()
        {
            IsFinished = true;
        }
    }
}