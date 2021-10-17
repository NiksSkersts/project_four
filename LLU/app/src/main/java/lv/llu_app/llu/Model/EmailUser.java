package lv.llu_app.llu.Model;

import android.database.Observable;
import android.widget.Toast;
import lv.llu_app.llu.Email.Email;
import lv.llu_app.llu.Tasks.GetTask;

import javax.mail.Message;
import javax.mail.MessagingException;
import java.util.ArrayList;
import java.util.List;

public class EmailUser extends User {
    //default server
    public final String host = "pop3.mailtrap.io";
    public final int port = 1100;
    public List<Message> messages = new ArrayList<>();
    public EmailUser(String username, String password) throws MessagingException {
            if (CheckConnection(username,password)){
                this.username = username;
                this.password = password;
                //todo find a better way to call TASK!
                //new GetTask();
            }
    }
    private boolean CheckConnection(String username, String password) throws MessagingException {
        return Email.GetStore(host, port, username, password).isConnected();
    }
}
