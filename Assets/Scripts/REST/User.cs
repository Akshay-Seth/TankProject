[System.Serializable]

public class User
{
    public string username;
    public string password;
    public int highScore;


}

[System.Serializable]
public class Highscore
{
    public User[] highScores;

}
