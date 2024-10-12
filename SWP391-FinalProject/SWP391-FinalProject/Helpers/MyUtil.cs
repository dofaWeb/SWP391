namespace SWP391_FinalProject.Helpers
{
    public class MyUtil
    {
        public static string UpLoadPicture(IFormFile picture)
        {
            try
            {
                var fullPathFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pictures", picture.FileName);
                using (var myfile = new FileStream(fullPathFile, FileMode.CreateNew))
                {
                    picture.CopyTo(myfile);
                }
                return picture.FileName;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static bool DeletePicture(string fileName)
        {
            try
            {
                var fullPathFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pictures", fileName);

                // Check if the file exists before attempting to delete it.
                if (File.Exists(fullPathFile))
                {
                    File.Delete(fullPathFile);
                    return true;  // File successfully deleted.
                }
                return false;  // File not found.
            }
            catch (Exception ex)
            {
                // Log the exception if needed.
                return false;  // Deletion failed.
            }
        }

    }
}
