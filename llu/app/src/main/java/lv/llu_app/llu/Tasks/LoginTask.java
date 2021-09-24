package lv.llu_app.llu.Tasks;

import java.io.IOException;

import javax.mail.MessagingException;

import lv.llu_app.llu.Email.Email;
import lv.llu_app.llu.Email.EmailTab;
import lv.llu_app.llu.Main.MainActivity;
import lv.llu_app.llu.Model.User;

public class LoginTask implements BaseTask {
    protected String username;
    protected String password;
    public boolean post;

    public LoginTask(String username, String password) {
        this.username = username;
        this.password = password;
        executor.execute(() -> {
            try {
                Object result = call();
                post = handler.post(() -> setDataAfterLoading(result));
            } catch (Exception e) {
                e.printStackTrace();
            }
        });
    }
    @Override
    public void setDataAfterLoading(Object result) {
        EmailTab.email_account = (Email) result;
    }
    @Override
    public Object call() throws MessagingException {
        Email result = new Email();
        result.User(username,password);
        return result;
    }
}
