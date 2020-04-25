using System.Linq;
using Domain.User;
using Infrastructure;
using WorkTimeAlghorithm;

namespace WindowUI.Profile
{
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
            _vm.Username = _authenticationService.User.Username;
        }
    }
}