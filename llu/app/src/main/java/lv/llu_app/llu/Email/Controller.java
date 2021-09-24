package lv.llu_app.llu.Email;

import java.io.IOException;

import javax.mail.Folder;
import javax.mail.Message;
import javax.mail.MessagingException;
import javax.mail.Store;

public interface Controller {
    void User(String username, String password) throws IOException, MessagingException;
    Store GetStore(String host, String username, String password) throws MessagingException;
    Folder GetFolder(Store store) throws MessagingException;
    Message[] GetMessages();
    Message GetMessages(int position);
    String Read(Message message) throws IOException, MessagingException;
}
