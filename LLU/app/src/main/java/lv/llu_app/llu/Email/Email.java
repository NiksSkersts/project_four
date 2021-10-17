package lv.llu_app.llu.Email;

import lv.llu_app.llu.Model.EmailUser;

import java.io.IOException;
import java.util.*;

import javax.mail.*;

public class Email{
    public static Store GetStore(String host,int port, String username, String password) throws MessagingException {
        Properties props = new Properties();
        //test server properties
        props.put("mail.pop3.port", port);
        props.put("mail.pop3.host", host);
        props.put("mail.pop3.user", username);
        props.put("mail.store.protocol", "pop3");
        //props.setProperty("mail.imap.ssl.enable", "true");
        Session session = Session.getInstance(props);
        Store store = session.getStore("pop3");
        //Store store = session.getStore("imap");
        store.connect(host, port, username, password);
        return store;
    }
    public static Folder GetFolder(Store store) throws MessagingException {
        Folder inbox = store.getFolder("INBOX");
        if (!inbox.isOpen())
            inbox.open(Folder.READ_ONLY);
        return inbox;
    }
    public static String Read(Message message) {
        String result = "";
        try {
            Object content = message.getContent();
            if (content instanceof Multipart) {
                StringBuilder messageContent = new StringBuilder();
                Multipart multipart = (Multipart) content;
                for (int i = 0; i < multipart.getCount(); i++) {
                    Part part = multipart.getBodyPart(i);
                    if (part.isMimeType("text/plain")) {
                        messageContent.append(part.getContent().toString());
                    }
                }
                result = messageContent.toString();
                result = content.toString();
            }
        }
        catch (MessagingException | IOException e){
            e.printStackTrace();
        }
        return result;
    }
}