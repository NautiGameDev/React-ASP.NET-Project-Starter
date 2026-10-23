import { GetActivityLogs } from "../services/activityLogService";
import { useState, useEffect } from 'react';
function ActivityLogs() {
    const [sysMessage, setSysMessage] = useState("Fetching activity logs...");
    const [logs, setLogs] = useState([]);

    useEffect(() => {
        const getLogs = async () => {
            const response = await GetActivityLogs();

            if (response.status === 200) {
                setLogs(response.data.activityLogs);
                setSysMessage("");
            }
            else {
                setSysMessage(`Error ${response.status}: ${response.message}`);
            }
        }

        getLogs();
    }, []);

  return (
      <div className="page">
          {sysMessage.trim() === "" ? (
                <div className="container">
                    <table>
                        <thead>
                            <tr>
                                  <th>
                                    UserName
                                  </th>
                                  <th>
                                    Email
                                  </th>
                                  <th>
                                    ActivityType
                                  </th>
                                  <th>
                                    IsSuccess
                                  </th>
                                  <th>
                                    Details
                                  </th>
                                  <th>
                                    UserClient
                                  </th>
                                  <th>
                                    TimeStamp
                                  </th>
                                  <th>
                                    Ip Address
                                  </th>
                                </tr>
                              </thead>

                      {logs.map((log, index) => (
                          <tr key={index} className={index % 2 === 0 ? "light-row" : "dark-row"} >
                                  <td>
                                      {log.userName}
                                  </td>
                                  <td>
                                      {log.email}
                                  </td>
                                  <td>
                                      {log.activityType}
                                  </td>
                                  <td>
                                      {log.isSuccess ? "True" : "False"}
                                  </td>
                                  <td>
                                      {log.details}
                                  </td>
                                  <td>
                                      {log.userClient}
                                  </td>
                                  <td>
                                      {new Date(log.timeStamp).toLocaleString('en-US', {
                                          dateStyle: 'medium',
                                          timeStyle: 'short',
                                      })}
                                  </td>
                                  <td>
                                      {log.ipAddress}
                                  </td>
                              </tr>
                          ))}
                  </table>
              </div>
          ) : (
                <h2>{sysMessage}</h2>
           )}
      </div>
  );
}

export default ActivityLogs;