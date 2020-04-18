using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AutoMapper;
using Domain.Services;
using Domain.User;
using Domain.WorkTimeAggregate;
using Infrastructure.Db;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.src.Services;
using Serilog;
using WorkTimeAlghorithm;
using WorkTimeAlghorithm.StateMachine;

namespace WMonitorAlghorithmTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WMonitorAlghorithm _alghorithm;
        private WorkTimeBuildService _workTimeBuildService;

        public MainWindow()
        {
            var config = new ConfigurationService("testsettings.json");
            var repo = new SqliteWorkTimeEsRepository(config,
                new MapperConfiguration(opt => opt.AddProfile<DbEventProfile>()).CreateMapper());

            var uow = new WorkTimeUow(repo);

            _workTimeBuildService = new WorkTimeBuildService(repo, new SqliteWorkTimeIdGeneratorService(config));
            var workTime =
                _workTimeBuildService.CreateStartedManually(new User("mpekala"), DateTime.UtcNow.AddMinutes(120));


            workTime.StartManually();

            _alghorithm = new WMonitorAlghorithm(new AlghorithmFaceRecognition(new HcFaceDetection(), new DnFaceRecognition(), new CaptureService(), new SqLiteTestImageRepository(config,
                new MapperConfiguration(opt => opt.AddProfile<DbTestImageProfile>()).CreateMapper())), new WorkTimeEventService(uow, repo, config), workTime);
            

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            _alghorithm.Start();
        }
    }
}
