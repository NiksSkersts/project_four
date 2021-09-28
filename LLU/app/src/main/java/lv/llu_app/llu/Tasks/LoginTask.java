package lv.llu_app.llu.Tasks;

import lv.llu_app.llu.Email.Email;
import lv.llu_app.llu.Email.EmailTab;

import javax.mail.AuthenticationFailedException;
import javax.mail.MessagingException;
import java.io.IOException;

public class LoginTask implements BaseTask {
    protected String username;
    protected String password;
    public boolean post;
    public boolean err = false;
    public LoginTask(String username, String password){
        this.username = username;
        this.password = password;
        executor.execute(() -> {
            try {
                Object result = call();
                post = handler.post(() -> setDataAfterLoading(result));
            } catch (Exception e) {
                e.printStackTrace();
                err =true;
            }
        });
    }
    @Override
    public Object call() throws IOException, MessagingException {
        Email result = new Email();
        result.User(username,password);
        return result;
    }

    @Override
    public void setDataAfterLoading(Object result) {
        EmailTab.email_account = (Email) result;
    }
}
