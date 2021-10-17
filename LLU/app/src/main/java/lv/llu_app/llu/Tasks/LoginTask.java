package lv.llu_app.llu.Tasks;

import android.widget.Toast;
import androidx.annotation.Nullable;
import androidx.annotation.StringRes;
import lv.llu_app.llu.Model.EmailUser;
import lv.llu_app.llu.Model.User;

import javax.mail.MessagingException;

public class LoginTask extends Task implements BaseTask {
    protected final String username;
    protected final String password;
    public LoginTask(String username, String password){
        this.username = username;
        this.password = password;
        executor.execute(() -> {
            try {
                Object result = call();
                if (result != null) {
                    post = handler.post(() -> setDataAfterLoading(result));
                }else{
                    err = true;
                }
            } catch (Exception e) {
                e.printStackTrace();
                err = true;
            }
        });
    }
    @Override
    public Object call(){
        //User can have multiple password for each service. EG. mans.llu.lv, lais.llu.lv, email and moodle;
        //Go through each of them and ask for info
        //todo Focus on email function
        try {
            return new EmailUser(username,password);
        } catch (MessagingException e) {
            e.printStackTrace();
        }
        return null;
    }

    @Override
    public void setDataAfterLoading(Object result) {
        User.email_account = (EmailUser) result;
    }
}
