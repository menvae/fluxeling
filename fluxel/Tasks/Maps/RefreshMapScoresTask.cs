using System.Collections.Generic;
using fluxel.Database.Helpers;
using fluxel.Tasks.Scores;
using fluxel.Tasks.Users;

namespace fluxel.Tasks.Maps;

public class RefreshMapScoresTask : ICronTask
{
    public string Name => "RefreshMapScores";

    public int Hour => 2;
    public int Minute => 0;
    public bool Valid { get; set; }

    public void Run()
    {
        var maps = MapHelper.NeedRefresh;
        var users = new List<long>();

        foreach (var map in maps)
        {
            var scores = ScoreHelper.FromMap(map, map.SHA256Hash);

            foreach (var score in scores)
            {
                ServerHost.Instance.Scheduler.Schedule(new RecalculateScoreTask(score.ID));
                if (!users.Contains(score.UserID)) users.Add(score.UserID);
            }

            MapHelper.QuickUpdate(map.ID, m => m.NeedsScoreRefresh = false);
        }

        foreach (var user in users)
        {
            ServerHost.Instance.Scheduler.Schedule(new RecalculateUserTask(user));
        }
    }
}
