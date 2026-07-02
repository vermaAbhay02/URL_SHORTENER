using System.Threading.Channels;
using URL_Shortener.Models;

namespace URL_Shortener.Services;

public class ClickLogQueue{
    private readonly Channel<ClickLog> _channel;  // thread safe in memory queue
    public ClickLogQueue(){
        _channel=Channel.CreateUnbounded<ClickLog>(); // no limit on how many items can sit in the queue waiting to be processed;
    }
    public void Enqueue(ClickLog log){    // the controller will call this to drop a clicklog into the pipe
        _channel.Writer.TryWrite(log);
    }
    public ChannelReader<ClickLog>Reader => _channel.Reader; 
    // exposed so the background processor can read from the other end
}