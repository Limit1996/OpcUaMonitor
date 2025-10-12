namespace OpcUaMonitor.Api.Endpoints;

public class DashboardEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/dashboard").WithTags("Dashboard");

        group.MapGet("", GetQueryPage)
            .WithName("GetQueryPage")
            .ExcludeFromDescription(); // 从 OpenAPI 文档中排除
    }

    private static IResult GetQueryPage()
    {
        var html = """
                   <!DOCTYPE html>
                   <html lang="zh-CN">
                   <head>
                       <meta charset="UTF-8">
                       <meta name="viewport" content="width=device-width, initial-scale=1.0">
                       <title>OPC UA 监控查询</title>
                       <style>
                           * { margin: 0; padding: 0; box-sizing: border-box; }
                           body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background: #f5f5f5; padding: 20px; }
                           .container { max-width: 1200px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
                           h1 { color: #333; margin-bottom: 30px; }
                           .section { margin-bottom: 30px; padding: 20px; border: 1px solid #e0e0e0; border-radius: 4px; }
                           .section h2 { color: #555; margin-bottom: 15px; font-size: 18px; }
                           .form-group { margin-bottom: 15px; }
                           label { display: block; margin-bottom: 5px; color: #666; font-weight: 500; }
                           input, textarea { width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 4px; font-size: 14px; }
                           textarea { min-height: 100px; resize: vertical; }
                           button { background: #007bff; color: white; padding: 10px 20px; border: none; border-radius: 4px; cursor: pointer; font-size: 14px; margin-right: 10px; }
                           button:hover { background: #0056b3; }
                           .result { margin-top: 20px; padding: 15px; background: #f8f9fa; border-radius: 4px; white-space: pre-wrap; font-family: monospace; }
                           .error { background: #f8d7da; color: #721c24; }
                           .success { background: #d4edda; color: #155724; }
                       </style>
                   </head>
                   <body>
                       <div class="container">
                           <h1>OPC UA 监控</h1>
                           
                           <!-- OPC UA 读取 -->
                           <div class="section">
                               <h2>📊 OPC UA 数据读取</h2>
                               <div class="form-group">
                                   <label>服务器地址:</label>
                                   <input type="text" id="opcUrl" placeholder="opc.tcp://localhost:49320" value="opc.tcp://localhost:49320">
                               </div>
                               <div class="form-group">
                                   <label>标签名称 (每行一个):</label>
                                   <textarea id="tagNames" placeholder="ns=2;s=通道 1.设备 1.标记 1&#10;ns=2;s=通道 1.设备 1.标记 2">ns=2;s=通道 1.设备 1.标记 1
                   ns=2;s=通道 1.设备 1.标记 2</textarea>
                               </div>
                               <button onclick="readOpcData()">读取数据</button>
                               <div id="opcResult" class="result" style="display:none;"></div>
                           </div>

                           <!-- 事件日志查询 -->
                           <div class="section">
                               <h2>📝 事件日志查询</h2>
                               <div class="form-group">
                                   <label>页码:</label>
                                   <input type="number" id="pageNumber" value="1" min="1">
                               </div>
                               <div class="form-group">
                                   <label>每页数量:</label>
                                   <input type="number" id="pageSize" value="20" min="1" max="100">
                               </div>
                               <div class="form-group">
                                   <label>事件 ID:</label>
                                   <input type="number" id="eventId" placeholder="0" value="0" disabled>
                               </div>
                               <button onclick="queryEventLogs()">查询日志</button>
                               <div id="logResult" class="result" style="display:none;"></div>
                           </div>
                       </div>

                       <script>
                           async function readOpcData() {
                               const url = document.getElementById('opcUrl').value;
                               const tagNames = document.getElementById('tagNames').value
                                   .split('\n')
                                   .map(t => t.trim())
                                   .filter(t => t);
                               
                               const resultDiv = document.getElementById('opcResult');
                               resultDiv.style.display = 'block';
                               resultDiv.className = 'result';
                               resultDiv.textContent = '正在读取...';

                               try {
                                   const params = new URLSearchParams();
                                   params.append('url', url);
                                   tagNames.forEach(tag => params.append('tagNames', tag));

                                   const response = await fetch(`/api/opcua/read?${params}`);
                                   const data = await response.json();
                                   
                                   resultDiv.className = 'result success';
                                   resultDiv.textContent = JSON.stringify(data, null, 2);
                               } catch (error) {
                                   resultDiv.className = 'result error';
                                   resultDiv.textContent = '错误: ' + error.message;
                               }
                           }

                           async function queryEventLogs() {
                               const pageNumber = document.getElementById('pageNumber').value;
                               const pageSize = document.getElementById('pageSize').value;
                               const eventId = document.getElementById('eventId').value;
                               
                               const resultDiv = document.getElementById('logResult');
                               resultDiv.style.display = 'block';
                               resultDiv.className = 'result';
                               resultDiv.textContent = '正在查询...';

                               try {
                                   const params = new URLSearchParams({
                                       pageNumber: pageNumber,
                                       pageSize: pageSize
                                   });
                                   if (eventId) params.append('type', eventId);

                                   const response = await fetch(`/api/event-logs?${params}`);
                                   const data = await response.json();
                                   
                                   resultDiv.className = 'result success';
                                   resultDiv.textContent = JSON.stringify(data, null, 2);
                               } catch (error) {
                                   resultDiv.className = 'result error';
                                   resultDiv.textContent = '错误: ' + error.message;
                               }
                           }
                       </script>
                   </body>
                   </html>
                   """;

        return Results.Content(html, "text/html", System.Text.Encoding.UTF8);
    }
}