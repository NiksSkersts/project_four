package lv.llu_app.llu.Tasks;

import java.util.Properties;

import javax.mail.Folder;
import javax.mail.Session;
import javax.mail.Store;

import lv.llu_app.llu.Model.User;

public class LoginTask extends BaseTask {
    Properties properties = null;
    private Session session = null;
    private Store store = null;
    private Folder inbox = null;
    protected String username = "";
    protected String password = "";

    public LoginTask(String username, String password) {
        this.username = username;
        this.password = password;
    }

    @Override
    public void setDataAfterLoading(Object result) {

    }

    @Override
    public Object call() throws Exception {

        Object result = null;
        result = TryLogin();//some network request for example
        return result;
    }

    public Object TryLogin() {
        properties = new Properties();
        properties.setProperty("mail.imap.ssl.enable", "true");
        Session session = Session.getInstance(properties);
        try {
            store = session.getStore("imap");
            store.connect("", username, password);
            if (!store.isConnected())
                return null;
            inbox = store.getFolder("INBOX");
            inbox.open(Folder.READ_ONLY);
            store.close();
            return new User(username, password);

        } catch (Exception e) {
            return null;
        }
    }
}
