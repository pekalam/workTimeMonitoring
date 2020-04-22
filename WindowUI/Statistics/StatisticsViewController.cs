namespace WindowUI.Statistics
{
    public class StatisticsViewController
    {
        private readonly WorkTimeModuleService _workTimeModuleService;
        private StatisticsViewModel _vm;

        public StatisticsViewController(WorkTimeModuleService workTimeModuleService)
        {
            _workTimeModuleService = workTimeModuleService;
        }

        public void Init(StatisticsViewModel vm)
        {
            _vm = vm;
        }
    }
}