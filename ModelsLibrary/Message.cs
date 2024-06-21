namespace ModelsLibrary;

public class Message
{
    public Message(string username, string body, bool mine)
    {
        Username = username;
        Body = body;
        Mine = mine;
    }

    public string Username { get; set; }
    public string Body { get; set; }
    public bool Mine { get; set; }
    public bool IsNotice => Body.StartsWith("[Achtung]");
    public string CSS => Mine ? "background-color: greenyellow" : "background-color: cornflowerblue";
}