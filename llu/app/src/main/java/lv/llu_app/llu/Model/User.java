package lv.llu_app.llu.Model;

public class User {
    private final String username;
    private final String password;

    public User(String username, String password) {
        this.username = username;
        this.password = password;
    }

    public String GetUsername() {
        return username;
    }
    public String GetPassword() {
        return password;
    }
}
