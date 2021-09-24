package lv.llu_app.llu.Model;

public class User{
    public final String host = "mail.llu.lv";
    protected final String username;
    protected final String password;

    public User(String username, String password) {
        this.username = username;
        this.password = password;
    }
}
