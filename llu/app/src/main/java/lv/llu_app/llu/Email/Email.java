package lv.llu_app.llu.Email;

import android.os.Looper;

import java.io.IOException;
import java.util.Properties;
import java.util.concurrent.Executor;
import java.util.concurrent.Executors;
import java.util.concurrent.atomic.AtomicReference;

import javax.mail.FetchProfile;
import javax.mail.Folder;
import javax.mail.Message;
import javax.mail.MessagingException;
import javax.mail.Multipart;
import javax.mail.Part;
import javax.mail.Session;
import javax.mail.Store;

import lv.llu_app.llu.Model.User;

public class Email implements Controller {
    private Message[] messages;
    public User user;
    public Store store;
    public Folder folder;
    @Override
    public void User(String username, String password) throws MessagingException {
        user = new User(username,password);
        store = GetStore(user.host,username,password);
        folder = GetFolder(store);
    }

    @Override
    public Store GetStore(String host, String username, String password) throws MessagingException {
        Properties properties = new Properties();
        properties.setProperty("mail.imap.ssl.enable", "true");
        Session session = Session.getInstance(properties);
        Store store = session.getStore("imap");
        store.connect(host, username, password);
        return store;
    }

    @Override
    public Folder GetFolder(Store store) throws MessagingException {
        Folder inbox = store.getFolder("INBOX");
        if (!inbox.isOpen())
            inbox.open(Folder.READ_ONLY);
        messages = inbox.getMessages();
        FetchProfile fp = new FetchProfile();
        fp.add(FetchProfile.Item.ENVELOPE);
        fp.add(FetchProfile.Item.CONTENT_INFO);
        fp.add(FetchProfile.Item.FLAGS);
        fp.add(FetchProfile.Item.SIZE);
        inbox.fetch(messages, fp);
        return inbox;
    }
    @Override
    public Message[] GetMessages(){
        return messages;
    }

    @Override
    public Message GetMessages(int position) {
        return messages[position];
    }

    @Override
    public String Read(Message message) throws IOException, MessagingException {
        AtomicReference<String> reply = new AtomicReference<>("");
        final android.os.Handler handler = new android.os.Handler(Looper.getMainLooper());
        Executor executor = Executors.newCachedThreadPool();
        executor.execute(() -> {
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
                }
                result = content.toString();
            } catch (MessagingException | IOException e) {
                e.printStackTrace();
            }
            String finalResult = result;
            handler.post(() -> reply.set(finalResult));
        });
        return reply.get();
    }
}
