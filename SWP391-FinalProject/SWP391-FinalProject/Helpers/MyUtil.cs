namespace SWP391_FinalProject.Helpers
{
    public class MyUtil
    {
        public static string UpLoadHinh(IFormFile picture)
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
    }
}
