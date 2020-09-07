namespace Celebrities.Database.Models
{
    public class Celebrity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FaceRecognitionName { get; set; }
        public string Info { get; set; }
        public byte[] AvatarImage { get; set; }
        public string ImageName { get; set; }
        public bool Trained { get; set; }
    }
}