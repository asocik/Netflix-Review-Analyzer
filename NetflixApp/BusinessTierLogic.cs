//
// Netflix Database Application using N-Tier Design. 
//
// Adam Socik
//

//
// BusinessTier:  business logic, acting as interface between UI and data store.
//

using System;
using System.Collections.Generic;
using System.Data;


namespace BusinessTier
{
    //
    // Business
    //
    public class Business
    {
        //
        // Fields
        //
        private string _DBFile;
        private DataAccessTier.Data datatier;

        //
        // Constructor
        //
        public Business(string DatabaseFilename)
        {
            _DBFile = DatabaseFilename;
            datatier = new DataAccessTier.Data(DatabaseFilename);
        }

        //
        // TestConnection
        //
        // Returns true if we can establish a connection to the database, false if not.
        //
        public bool TestConnection()
        {
            return datatier.TestConnection();
        }

        //
        // GetMovie
        //
        // Retrieves Movie object based on MOVIE ID; returns null if movie is not
        // found.
        //
        public Movie GetMovie(int MovieID)
        {
            string sql = string.Format("SELECT MovieName FROM Movies WHERE MovieID={0};", MovieID);
            object result = datatier.ExecuteScalarQuery(sql);

            if (result == null || result.ToString() == "")
                return null;
            return new Movie(MovieID, result.ToString());
        }

        //
        // GetMovie
        //
        // Retrieves Movie object based on MOVIE NAME; returns null if movie is not
        // found.
        //
        public Movie GetMovie(string MovieName)
        {
            string sql = string.Format("SELECT MovieID FROM Movies WHERE MovieName='{0}';", MovieName);
            object result = datatier.ExecuteScalarQuery(sql);

            if (result == null || result.ToString() == "")
                return null;
            return new Movie(System.Convert.ToInt32(result.ToString()), MovieName);
        }

        //
        // AddMovie
        //
        // Adds the movie, returning a Movie object containing the name and the 
        // movie's id.  If the add failed, null is returned.
        //
        public Movie AddMovie(string MovieName)
        {
            string sql = string.Format(@"
                INSERT INTO Movies(MovieName) Values('{0}');
                SELECT MovieID FROM Movies WHERE MovieID = SCOPE_IDENTITY();", MovieName);
            object result = datatier.ExecuteScalarQuery(sql);

            if (result == null)
                return null;

            return new Movie(System.Convert.ToInt32(result.ToString()), MovieName);
        }

        //
        // AddReview
        //
        // Adds review based on MOVIE ID, returning a Review object containing
        // the review, review's id, etc.  If the add failed, null is returned.
        //
        public Review AddReview(int MovieID, int UserID, int Rating)
        {
            string sql = string.Format(@"
                INSERT INTO Reviews(MovieID, UserID, Rating) Values({0}, {1}, {2});
                SELECT ReviewID FROM Reviews WHERE ReviewID = SCOPE_IDENTITY();",
                MovieID, UserID, Rating);
            object result = datatier.ExecuteScalarQuery(sql);

            if (result == null)
                return null;

            return new Review(System.Convert.ToInt32(result.ToString()), MovieID, UserID, Rating);
        }

        //
        // GetMovieDetail
        //
        // Given a MOVIE ID, returns detailed information about this movie --- all
        // the reviews, the total number of reviews, average rating, etc.  If the 
        // movie cannot be found, null is returned.
        //
        public MovieDetail GetMovieDetail(int MovieID)
        {
            Movie movie = GetMovie(MovieID);
            if (movie == null)
                return null;

            // Find the average rating of the movie
            string sql = string.Format(@"
                SELECT ROUND(AVG(CAST(Rating AS Float)), 4) AS AvgRating 
                FROM Reviews
				INNER JOIN Movies ON Reviews.MovieID = Movies.MovieID
				WHERE Movies.MovieName='{0}';", (movie.MovieName).Replace("'", "''"));
            object result = datatier.ExecuteScalarQuery(sql);
            double avgRating = 0;
            if (result != null)
                avgRating = System.Convert.ToDouble(result.ToString());

            // Get all of the reviews for the movie
            int numReviews = 0;
            List<Review> reviews = new List<Review>();

            sql = string.Format(@"
                SELECT ReviewID, UserID, Rating 
                FROM Reviews 
                WHERE MovieID={0}
                ORDER BY Rating Desc, UserID ASC;", MovieID);
            DataSet ds = datatier.ExecuteNonScalarQuery(sql);

            // If the data set was empty then return with empty results (movie, 0, 0, empty list)
            if (ds == null)
                return new MovieDetail(movie, avgRating, numReviews, reviews.AsReadOnly());

            DataTable dt = ds.Tables["TABLE"];

            // Create the reviews list
            foreach (DataRow row in dt.Rows)
            {
                int reviewid = System.Convert.ToInt32((row["ReviewID"]).ToString());
                int userid = System.Convert.ToInt32((row["UserID"]).ToString());
                int rating = System.Convert.ToInt32((row["Rating"]).ToString());
                reviews.Add(new Review(reviewid, MovieID, userid, rating));
                numReviews++;
            }

            return new MovieDetail(movie, avgRating, numReviews, reviews.AsReadOnly());
        }

        //
        // GetUserDetail
        //
        // Given a USER ID, returns detailed information about this user --- all
        // the reviews submitted by this user, the total number of reviews, average 
        // rating given, etc.  If the user cannot be found, null is returned.
        //
        public UserDetail GetUserDetail(int UserID)
        {
            User user = new User(UserID);
            double avgRating = 0;
            int numReviews = 0;
            List<Review> reviews = new List<Review>();

            // Find the count of ratings
            string sql = string.Format(@"SELECT COUNT(Rating) FROM Reviews WHERE UserID={0};", UserID);
            object result = datatier.ExecuteScalarQuery(sql);

            // User could not be found
            if (result == null)
                return null;

            numReviews = System.Convert.ToInt32(result.ToString());

            // Get the average rating for the user
            sql = string.Format(@"SELECT AVG(Rating) FROM Reviews WHERE UserID={0}", UserID);
            result = datatier.ExecuteScalarQuery(sql);
            avgRating = System.Convert.ToInt32(result.ToString());

            sql = string.Format(@"SELECT ReviewID, MovieID, Rating FROM Reviews WHERE UserID={0}", UserID);
            DataSet ds = datatier.ExecuteNonScalarQuery(sql);

            // If the data set was empty then return with empty results 
            if (ds == null)
                return new UserDetail(user, avgRating, numReviews, reviews.AsReadOnly());

            // Create the reviews list
            DataTable dt = ds.Tables["TABLE"];
            foreach (DataRow row in dt.Rows)
            {
                int reviewid = System.Convert.ToInt32((row["ReviewID"]).ToString());
                int movieid = System.Convert.ToInt32((row["MovieID"]).ToString());
                int rating = System.Convert.ToInt32((row["Rating"]).ToString());
                reviews.Add(new Review(reviewid, movieid, UserID, rating));
            }

            return new UserDetail(user, avgRating, numReviews, reviews.AsReadOnly());
        }

        //
        // GetTopMoviesByAvgRating
        //
        // Returns the top N movies in descending order by average rating.  If two
        // movies have the same rating, the movies are presented in ascending order
        // by name.  If N < 1, an EMPTY LIST is returned.
        //
        public IReadOnlyList<Movie> GetTopMoviesByAvgRating(int N)
        {
            List<Movie> movies = new List<Movie>();
            string sql = string.Format(@"
                SELECT TOP {0} Movies.MovieName, Movies.MovieID 
                FROM Movies
                INNER JOIN 
                (
                    SELECT MovieID, ROUND(AVG(CAST(Rating AS Float)), 4) as AvgRating 
                    FROM Reviews
                    GROUP BY MovieID
                ) g
                ON g.MovieID = Movies.MovieID
                ORDER BY g.AvgRating DESC, Movies.MovieName Asc;", N);
            DataSet ds = datatier.ExecuteNonScalarQuery(sql);

            // If the data set was empty then return with empty results 
            if (ds == null)
                return movies;

            // Create the reviews list
            DataTable dt = ds.Tables["TABLE"];
            foreach (DataRow row in dt.Rows)
            {
                string movieName = (row["MovieName"]).ToString();
                int movieID = System.Convert.ToInt32((row["MovieID"]).ToString());
                movies.Add(new Movie(movieID, movieName));
            }

            return movies;
        }

        //
        // GetTopMoviesByNumReviews
        //
        // Returns the top N movies in descending order by number of reviews.  If two
        // movies have the same number of reviews, the movies are presented in ascending
        // order by name.  If N < 1, an EMPTY LIST is returned.
        //
        public IReadOnlyList<Movie> GetTopMoviesByNumReviews(int N)
        {
            List<Movie> movies = new List<Movie>();
            string sql = string.Format(@"
                SELECT TOP {0} Movies.MovieName, Movies.MovieID 
                FROM Movies
                INNER JOIN
                (
                    SELECT MovieID, COUNT(*) as RatingCount 
                    FROM Reviews
                    GROUP BY MovieID
                ) g
                ON g.MovieID = Movies.MovieID
                ORDER BY g.RatingCount DESC, Movies.MovieName Asc;", N);

            DataSet ds = datatier.ExecuteNonScalarQuery(sql);

            // If the data set was empty then return with empty results 
            if (ds == null)
                return movies;

            // Create the reviews list
            DataTable dt = ds.Tables["TABLE"];
            foreach (DataRow row in dt.Rows)
            {
                string movieName = (row["MovieName"]).ToString();
                int movieID = System.Convert.ToInt32((row["MovieID"]).ToString());
                movies.Add(new Movie(movieID, movieName));
            }
            
            return movies;
        }

        //
        // GetTopUsersByNumReviews
        //
        // Returns the top N users in descending order by number of reviews.  If two
        // users have the same number of reviews, the users are presented in ascending
        // order by user id.  If N < 1, an EMPTY LIST is returned.
        //
        public IReadOnlyList<User> GetTopUsersByNumReviews(int N)
        {
            List<User> users = new List<User>();
            string sql = string.Format(@"
                SELECT TOP {0} UserID, COUNT(*) AS RatingCount
                FROM Reviews
                GROUP BY UserID
                ORDER BY RatingCount DESC, UserID Asc;", N);
            DataSet ds = datatier.ExecuteNonScalarQuery(sql);

            // If the data set was empty then return with empty results 
            if (ds == null)
                return users;

            // Create the reviews list
            DataTable dt = ds.Tables["TABLE"];
            foreach (DataRow row in dt.Rows)
            {
                int userid = System.Convert.ToInt32((row["UserID"]).ToString());
                users.Add(new User(userid));
            }
      
            return users;
        }

        //
        // GetAllMovies
        //
        // Returns a list of all movies in the database in alphabetical order
        //
        public IReadOnlyList<Movie> GetAllMovies()
        {
            List<Movie> movies = new List<Movie>();
            string sql = string.Format(@"SELECT * FROM Movies");
            DataSet ds = datatier.ExecuteNonScalarQuery(sql);

            // If the data set was empty then return with empty results 
            if (ds == null)
                return null;

            // Create the reviews list
            DataTable dt = ds.Tables["TABLE"];
            foreach (DataRow row in dt.Rows)
            {
                string movieName = (row["MovieName"]).ToString();
                int movieID = System.Convert.ToInt32((row["MovieID"]).ToString());
                movies.Add(new Movie(movieID, movieName));
            }

            return movies;
        }

    }//class
}//namespace
