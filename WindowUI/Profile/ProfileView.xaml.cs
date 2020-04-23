using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Domain.User;
using Infrastructure;
using Prism.Mvvm;
using Prism.Regions;
using WorkTimeAlghorithm;

namespace WindowUI.Profile
{
    public static class LinqExtensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            var coll = new ObservableCollection<T>();

            foreach (var item in source)
            {
                coll.Add(item);   
            }

            return coll;
        }
    }

    /// <summary>
    /// Interaction logic for ProfileView
    /// </summary>
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            InitializeComponent();
        }
    }

    public class ProfileViewModel : BindableBase, INavigationAware
    {
        private readonly IProfileViewController _controller;
        private ObservableCollection<BitmapImage> _referenceImgs;

        public ProfileViewModel(IProfileViewController controller)
        {
            _controller = controller;
        }

        public ObservableCollection<BitmapImage> ReferenceImgs
        {
            get => _referenceImgs;
            set => SetProperty(ref _referenceImgs, value);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _controller.Init(this);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }

    public interface IProfileViewController
    {
        void Init(ProfileViewModel vm);
    }

    public class ProfileViewController : IProfileViewController
    {
        private readonly ITestImageRepository _testImageRepository;
        private readonly IAuthenticationService _authenticationService;
        private ProfileViewModel _vm;

        public ProfileViewController(ITestImageRepository testImageRepository, IAuthenticationService authenticationService)
        {
            _testImageRepository = testImageRepository;
            _authenticationService = authenticationService;

        }


        public void Init(ProfileViewModel vm)
        {
            _vm = vm;

            var refImgs = _testImageRepository.GetReferenceImages(_authenticationService.User);
            _vm.ReferenceImgs = refImgs.Select(f => f.Img.ToBitmapImage()).ToObservableCollection();
        }
    }
}
