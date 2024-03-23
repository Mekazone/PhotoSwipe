using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Swiper.Utils;

namespace Swiper.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SwiperControl : ContentView
    {
        private readonly double _initialRotation;
        private static readonly Random _random = new Random();
        //dead zone when panning an image. If we release the
        //image in this zone, it simply returns to the center of the screen without any action being taken
        private const double DeadZone = 0.4d;
        //used for interpolating the opacity of the StackLayout on either side of the layout
        private const double DecisionThreshold = 0.4d;

        //used to store the width as soon as we have resolved it.
        private double _screenWidth = -1;
        //events for like and deny
        public event EventHandler OnLike;
        public event EventHandler OnDeny;

        public SwiperControl()
        {
            InitializeComponent();

            //wire up the pan gesture and set some initial rotation values
            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            this.GestureRecognizers.Add(panGesture);
            _initialRotation = _random.Next(-10, 10);
            photo.RotateTo(_initialRotation, 100, Easing.SinOut);

            var picture = new Picture();
            descriptionLabel.Text = picture.Description;
            image.Source = new UriImageSource() { Uri = picture.Uri };

            //make the loading lable disappear when image finishes loading
            loadingLabel.SetBinding(IsVisibleProperty, "IsLoading");
            loadingLabel.BindingContext = image;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (Application.Current.MainPage == null)
            {
                return;
            }
            _screenWidth = Application.Current.MainPage.Width;
        }

        private static double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        private void CalculatePanState(double panX)
        {
            var halfScreenWidth = _screenWidth / 2;
            var deadZoneEnd = DeadZone * halfScreenWidth;
            if (Math.Abs(panX) < deadZoneEnd)
            {
                return;
            }
            var passedDeadzone = panX < 0 ? panX + deadZoneEnd : panX -
            deadZoneEnd;
            var decisionZoneEnd = DecisionThreshold * halfScreenWidth;
            var opacity = passedDeadzone / decisionZoneEnd;
            opacity = Clamp(opacity, -1, 1);
            likeStackLayout.Opacity = opacity;
            denyStackLayout.Opacity = -opacity;
        }

        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    PanStarted();
                    break;
                case GestureStatus.Running:
                    PanRunning(e);
                    break;
                case GestureStatus.Completed:
                    PanCompleted();
                    break;
            }
        }

        //determines if an image has panned far enough for it to count as an exit of that image
        private bool CheckForExitCriteria()
        {
            var halfScreenWidth = _screenWidth / 2;
            var decisionBreakpoint = DeadZone * halfScreenWidth;
            return (Math.Abs(photo.TranslationX) > decisionBreakpoint);
        }
        private void PanStarted()
        {
            photo.ScaleTo(1.1, 100);
        }
        private void PanRunning(PanUpdatedEventArgs e)
        {
            photo.TranslationX = e.TotalX;
            photo.TranslationY = e.TotalY;
            photo.Rotation = _initialRotation + (photo.TranslationX / 25);
            //passes the total amount of movement on the x axis to the CalculatePanState() method, to determine if we need to adjust the opacity of
            //either the StackLayout on the right or the left of the control.
            CalculatePanState(e.TotalX);

        }
        private void PanCompleted()
        {
            //If the exit criteria are not met, we need to reset the state and the opacity of the StackLayout
            if (CheckForExitCriteria())
            {
                Exit();
            }
            likeStackLayout.Opacity = 0;
            denyStackLayout.Opacity = 0;

            photo.TranslateTo(0, 0, 250, Easing.SpringOut);
            photo.RotateTo(_initialRotation, 250, Easing.SpringOut);
            photo.ScaleTo(1, 250);
        }

        //Remove image from page
        private void Exit()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var direction = photo.TranslationX < 0 ? -1 : 1;
                if (direction > 0)
                {
                    OnLike?.Invoke(this, new EventArgs());
                }
                if (direction < 0)
                {
                    OnDeny?.Invoke(this, new EventArgs());
                }
                await photo.TranslateTo(photo.TranslationX + (_screenWidth * direction), photo.TranslationY, 200, Easing.CubicIn);
                var parent = Parent as Layout<View>;
                parent?.Children.Remove(this);
            });
        }
    }
}