//
// Netflix Database Application using N-Tier Design. 
//
// Adam Socik
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetflixApp
{
	public partial class Form1 : Form
	{
		//
		// Class members:
		//
		private Random RandomNumberGenerator;

		//
		// Constructor:
		//
		public Form1()
		{
			InitializeComponent();
			RandomNumberGenerator = new Random();
		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}

        //
        // Test Connection
        //
        private void cmdConnect_Click(object sender, EventArgs e)
        {
            BusinessTier.Business bt = new BusinessTier.Business(this.txtFileName.Text);
            if (bt.TestConnection() == true)
                MessageBox.Show("Connected");
            else
                MessageBox.Show("Not connected");
        }

        //
        // Get Movie Name:  from id
        //
		private void cmdGetMovieName_Click(object sender, EventArgs e)
		{
            listBox1.Items.Clear();
            int id = System.Convert.ToInt32(this.txtMovieID.Text);

            BusinessTier.Business bt = new BusinessTier.Business("netflix.mdf");
            BusinessTier.Movie movie = bt.GetMovie(id);

            if (movie == null)
                listBox1.Items.Add("Movie not found...");
            else
                listBox1.Items.Add(movie.MovieName);
        }

        //
        // Get Movie Reviews
        //
		private void cmdGetMovieReviews_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            int id = System.Convert.ToInt32(this.txtMovieID.Text);

            BusinessTier.Business bt = new BusinessTier.Business("netflix.mdf");
            BusinessTier.Movie movie = bt.GetMovie(id);

            if (movie == null)
                listBox1.Items.Add("Movie not found...");
            else
            {
                var detail = bt.GetMovieDetail(movie.MovieID);
                foreach (var review in detail.Reviews)
                    listBox1.Items.Add(review.UserID + ": " + review.Rating);
            }
        }

		//
		// Average Rating
		//
		private void cmdAvgRating_Click(object sender, EventArgs e)
		{
            listBox1.Items.Clear();
            string name = txtRatingsMovieName.Text;
            name = name.Replace("'", "''");

            BusinessTier.Business bt = new BusinessTier.Business("netflix.mdf");
            BusinessTier.Movie movie = bt.GetMovie(name);

            if (movie == null)
                listBox1.Items.Add("Movie not found...");
            else
            {
                var detail = bt.GetMovieDetail(movie.MovieID);
                listBox1.Items.Add("Average rating: " + detail.AvgRating);
            }
		}

		//
		// Each Rating
		//
		private void cmdEachRating_Click(object sender, EventArgs e)
		{
            listBox1.Items.Clear();
            string name = txtRatingsMovieName.Text;
            name = name.Replace("'", "''");

            BusinessTier.Business bt = new BusinessTier.Business("netflix.mdf");
            BusinessTier.Movie movie = bt.GetMovie(name);

            if (movie == null)
                listBox1.Items.Add("Movie not found...");
            else
            {
                var detail = bt.GetMovieDetail(movie.MovieID);
                int ones = 0;
                int twos = 0;
                int threes = 0;
                int fours = 0;
                int fives = 0;

                foreach(var review in detail.Reviews)
                {
                    switch (review.Rating)
                    {
                        case 1:
                            ones++;
                            break;
                        case 2:
                            twos++;
                            break;
                        case 3:
                            threes++;
                            break;
                        case 4:
                            fours++;
                            break;
                        case 5:
                            fives++;
                            break;
                    }
                }
                listBox1.Items.Add("5: " + fives);
                listBox1.Items.Add("4: " + fours);
                listBox1.Items.Add("3: " + threes);
                listBox1.Items.Add("2: " + twos);
                listBox1.Items.Add("1: " + ones);
                listBox1.Items.Add("Total: " + (ones + twos + threes + fours + fives));
            }
		}
	
		//
		// Add movie
		//
		private void cmdInsertMovie_Click(object sender, EventArgs e)
		{
			listBox1.Items.Clear();
            string name = txtInsertMovieName.Text;
            name = name.Replace("'", "''"); 

            BusinessTier.Business bt = new BusinessTier.Business("netflix.mdf");
            BusinessTier.Movie movie = bt.AddMovie(name);

            if (movie != null)
                listBox1.Items.Add("Success, movie added with id: " + movie.MovieID);
            else
                listBox1.Items.Add("** Insert failed?! **");
		}

		private void tbarRating_Scroll(object sender, EventArgs e)
		{
			lblRating.Text = tbarRating.Value.ToString();
		}

		//
		// Add Review
		//
		private void cmdInsertReview_Click(object sender, EventArgs e)
		{
            listBox1.Items.Clear();
            string name = txtInsertMovieName.Text;
            name = name.Replace("'", "''");

            BusinessTier.Business bt = new BusinessTier.Business("netflix.mdf");
            BusinessTier.Movie movie = bt.GetMovie(name);

            if (movie == null)
                listBox1.Items.Add("Movie not found...");
            else
            {
                int userid = RandomNumberGenerator.Next(100000, 999999);  // 6-digit user ids:
                BusinessTier.Review review = bt.AddReview(movie.MovieID, userid, System.Convert.ToInt32(lblRating.Text));

                if (review != null)
                    listBox1.Items.Add("Success, review added at id: " + review.ReviewID);
                else
                    listBox1.Items.Add("** Insert failed?! **");
            }
	    }

        //
        // Top N Movies by Average Rating
        //
        private void cmdTopMoviesByAvgRating_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            string N = txtTopN.Text; 

            BusinessTier.Business bt = new BusinessTier.Business("netflix.mdf");
            IReadOnlyList<BusinessTier.Movie> movies = bt.GetTopMoviesByAvgRating(Int32.Parse(N));

            if (movies.Count() == 0)
            {
                listBox1.Items.Add("**Error, or database is empty?!");
            }
            else
            {
                foreach (var movie in movies)
                {
                    var detail = bt.GetMovieDetail(movie.MovieID);
                    listBox1.Items.Add(detail.AvgRating + "\t" + movie.MovieName);
                }
            }
        }

        //
        // Top N Movies by # of reviews
        //
        private void cmdTopMoviesByNumReviews_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            string N = txtTopN.Text;

            BusinessTier.Business bt = new BusinessTier.Business("netflix.mdf");
            IReadOnlyList<BusinessTier.Movie> movies = bt.GetTopMoviesByNumReviews(Int32.Parse(N));

            if (movies.Count() == 0)
            {
                listBox1.Items.Add("**Error, or database is empty?!");
            }
            else
            {
                foreach (var movie in movies)
                {
                    var detail = bt.GetMovieDetail(movie.MovieID);
                    listBox1.Items.Add(detail.NumReviews + "\t" + movie.MovieName);
                }
            }
        }

        //
        // Top N Users by # of reviews
        //
        private void cmdTopUsers_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            string N = txtTopN.Text;

            BusinessTier.Business bt = new BusinessTier.Business("netflix.mdf");
            IReadOnlyList<BusinessTier.User> users = bt.GetTopUsersByNumReviews(Int32.Parse(N));

            if (users.Count() == 0)
            {
                listBox1.Items.Add("**Error, or database is empty?!");
            }
            else
            {
                foreach (var user in users)
                {
                    var detail = bt.GetUserDetail(user.UserID);
                    listBox1.Items.Add(detail.NumReviews + "\t" + user.UserID);
                }
            }
        }

        private void printMovies_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            BusinessTier.Business bt = new BusinessTier.Business("netflix.mdf");
            IReadOnlyList<BusinessTier.Movie> movies = bt.GetAllMovies();


            if (movies == null)
                listBox1.Items.Add("Movies not found...");
            else
            {
                foreach(var movie in movies)
                    listBox1.Items.Add(movie.MovieID + "\t" + movie.MovieName);
            }
        }

        private void txtMovieID_TextChanged(object sender, EventArgs e)
        {

        }
	}//class
}//namespace
