using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Swiper.Controls;

namespace Swiper
{
    public partial class MainPage : ContentPage
    {
        //keeps track of number of likes and dislikes
        private int _likeCount;
        private int _denyCount;

        //assigns the values of the labels to the fields above
        private void UpdateGui()
        {
            likeLabel.Text = _likeCount.ToString();
            denyLabel.Text = _denyCount.ToString();
        }

        //event handler
        private void Handle_OnLike(object sender, EventArgs e)
        {
            _likeCount++;
            InsertPhoto();
            UpdateGui();
        }

        //event handler
        private void Handle_OnDeny(object sender, EventArgs e)
        {
            _denyCount++;
            InsertPhoto();
            UpdateGui();
        }

        //will be called upon startup. This method simply calls the InsertPhoto() method 10 times and adds a new
        //SwiperControl to the MainGrid each time.
        private void AddInitialPhotos()
        {
            for (int i = 0; i < 10; i++)
            {
                InsertPhoto();
            }
        }

        private void InsertPhoto()
        {
            var photo = new SwiperControl();
            photo.OnDeny += Handle_OnDeny;
            photo.OnLike += Handle_OnLike;
            this.MainGrid.Children.Insert(0, photo);
        }

        public MainPage()
        {
            InitializeComponent();
            AddInitialPhotos();

        }
    }
}
