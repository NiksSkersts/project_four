package lv.llu_app.llu.Tasks;

import androidx.annotation.Nullable;
import lv.llu_app.llu.Email.Email;
import lv.llu_app.llu.Model.EmailUser;
import lv.llu_app.llu.Model.User;

import javax.mail.Folder;
import javax.mail.Message;
import javax.mail.MessagingException;
import javax.mail.Store;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;

public class GetTask extends Task implements BaseTask{
    private Object ArrayList = new ArrayList<>();
    public GetTask(){
        executor.execute(()->{
            try {
                ArrayList = call();
                post = handler.post(()->setDataAfterLoading(ArrayList));
            } catch (IOException | MessagingException e) {
                e.printStackTrace();
                err = true;
            }
        });
    }
    @Nullable
    @org.jetbrains.annotations.Nullable
    @Override
    public Object call() throws IOException, MessagingException {
        EmailUser emailUser = User.email_account;
        Store store = Email.GetStore(emailUser.host,emailUser.port,emailUser.username, emailUser.password);
        Folder folder = Email.GetFolder(store);
        Message[] messages = folder.getMessages();
        return new ArrayList<>(Arrays.asList(messages));
    }

    @Override
    public void setDataAfterLoading(Object result) {
        EmailUser emailUser = User.email_account;
        emailUser.messages = (ArrayList<Message>) result;
        emailUser.messages.notifyAll();
    }
}
