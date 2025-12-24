namespace OpcUaMonitor.FastEndpointApi.EventLog;

public class HtmlEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/EventLog");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var htmlContent = """
                          <!DOCTYPE html>
                          <html lang="zh-CN">
                          <head>
                              <meta charset="utf-8" />
                              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                              <title>事件日志查询</title>
                              <script src="/js/tailwindcss.js"></script>
                              <script>
                                  tailwind.config = {
                                      theme: {
                                          extend: {
                                              colors: {
                                                  border: "hsl(214.3 31.8% 91.4%)",
                                                  input: "hsl(214.3 31.8% 91.4%)",
                                                  ring: "hsl(215 20.2% 65.1%)",
                                                  background: "hsl(0 0% 100%)",
                                                  foreground: "hsl(222.2 84% 4.9%)",
                                                  primary: {
                                                      DEFAULT: "hsl(222.2 47.4% 11.2%)",
                                                      foreground: "hsl(210 40% 98%)",
                                                  },
                                                  muted: {
                                                      DEFAULT: "hsl(210 40% 96.1%)",
                                                      foreground: "hsl(215.4 16.3% 46.9%)",
                                                  },
                                                  card: {
                                                      DEFAULT: "hsl(0 0% 100%)",
                                                      foreground: "hsl(222.2 84% 4.9%)",
                                                  },
                                              },
                                              borderRadius: {
                                                  lg: "0.5rem",
                                                  md: "calc(0.5rem - 2px)",
                                                  sm: "calc(0.5rem - 4px)",
                                              },
                                          },
                                      },
                                  }
                              </script>
                              <style>
                                  body { font-family: "Inter", ui-sans-serif, system-ui, -apple-system, sans-serif; }
                                  input[type="datetime-local"]::-webkit-calendar-picker-indicator {
                                      cursor: pointer;
                                  }
                              </style>
                          </head>
                          <body class="bg-background text-foreground p-6 min-h-screen flex flex-col items-center">
                              <div class="w-full max-w-7xl space-y-6">
                                  <div class="space-y-1">
                                      <h1 class="text-2xl font-bold tracking-tight">事件日志查询</h1>
                                      <p class="text-muted-foreground text-sm">查看和筛选 OPC UA 历史事件记录。</p>
                                  </div>

                                  <div class="rounded-lg border bg-card text-card-foreground shadow-sm">
                                      <div class="p-6">
                                          <form id="query-form" class="grid gap-x-6 gap-y-4 md:grid-cols-2 lg:grid-cols-3">
                                              <div class="flex items-center gap-3">
                                                  <label class="w-20 text-sm font-medium text-right shrink-0">设备名称</label>
                                                  <input type="text" name="deviceName" placeholder="输入设备名称..."
                                                         class="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring">
                                              </div>

                                              <div class="flex items-center gap-3">
                                                  <label class="w-20 text-sm font-medium text-right shrink-0">标签备注</label>
                                                  <input type="text" name="tagRemark" placeholder="输入备注关键词..."
                                                         class="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring">
                                              </div>

                                              <div class="flex items-center gap-3">
                                                  <label class="w-20 text-sm font-medium text-right shrink-0">数值筛选</label>
                                                  <input type="text" name="values" placeholder="例如: 1, 0 (逗号分隔)"
                                                         class="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring">
                                              </div>

                                              <div class="flex items-center gap-3">
                                                  <label class="w-20 text-sm font-medium text-right shrink-0">开始时间</label>
                                                  <input type="datetime-local" name="startTime" required
                                                         class="h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring">
                                              </div>

                                              <div class="flex items-center gap-3">
                                                  <label class="w-20 text-sm font-medium text-right shrink-0">结束时间</label>
                                                  <input type="datetime-local" name="endTime" required
                                                         class="h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring">
                                              </div>

                                              <div class="flex items-center justify-end">
                                                  <button type="submit"
                                                          class="inline-flex items-center justify-center whitespace-nowrap rounded-md text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50 bg-primary text-primary-foreground shadow hover:bg-primary/90 h-9 px-6 py-2 w-full md:w-auto">
                                                      查询日志
                                                  </button>
                                              </div>
                                          </form>
                                      </div>
                                  </div>

                                  <div id="status" class="text-sm text-muted-foreground h-5 flex items-center px-1"></div>

                                  <div class="rounded-md border bg-card">
                                      <div class="relative w-full overflow-auto">
                                          <table class="w-full caption-bottom text-sm">
                                              <thead class="[&_tr]:border-b">
                                                  <tr class="border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted">
                                                      <th class="h-10 px-4 text-left align-middle font-medium text-muted-foreground">设备名称</th>
                                                      <th class="h-10 px-4 text-left align-middle font-medium text-muted-foreground">标签地址</th>
                                                      <th class="h-10 px-4 text-left align-middle font-medium text-muted-foreground">备注</th>
                                                      <th class="h-10 px-4 text-left align-middle font-medium text-muted-foreground">值</th>
                                                      <th class="h-10 px-4 text-left align-middle font-medium text-muted-foreground">Server时间戳</th>
                                                      <th class="h-10 px-4 text-left align-middle font-medium text-muted-foreground">Metadata</th>
                                                  </tr>
                                              </thead>
                                              <tbody id="result-body" class="[&_tr:last-child]:border-0">
                                                  <tr class="border-b transition-colors hover:bg-muted/50">
                                                      <td colspan="6" class="p-4 text-center text-muted-foreground">暂无数据，请点击查询</td>
                                                  </tr>
                                              </tbody>
                                          </table>
                                      </div>
                                  </div>
                              </div>

                              <script>
                                  const form = document.getElementById('query-form');
                                  const resultBody = document.getElementById('result-body');
                                  const statusEl = document.getElementById('status');
                                  const endpoint = '/api/event-log/list';

                                  const pad = (n) => String(n).padStart(2, '0');
                                  const toLocalInput = (date) =>
                                      `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;

                                  const initDefaults = () => {
                                      const end = new Date();
                                      const start = new Date(end.getTime() - 24 * 60 * 60 * 1000);
                                      form.startTime.value = toLocalInput(start);
                                      form.endTime.value = toLocalInput(end);
                                  };

                                  const formatParams = (params) => {
                                      if (!params) return '-';
                                      try {
                                          const obj = typeof params === 'string' ? JSON.parse(params) : params;
                                          const entries = Object.entries(obj).slice(0, 2);
                                          if (entries.length === 0) return '-';
                                          return entries.map(([key, value]) => {
                                              if (typeof value === 'string' && !isNaN(Date.parse(value))) {
                                                  return `${key}:${new Date(value).toLocaleString('zh-CN')}`;
                                              }
                                              return `${key}:${value}`;
                                          }).join(', ');
                                      } catch {
                                          return '-';
                                      }
                                  };
                          
                                  const renderRows = (logs) => {
                                      if (!logs || !logs.length) {
                                          resultBody.innerHTML = '<tr><td colspan="6" class="p-4 text-center text-muted-foreground">无匹配数据</td></tr>';
                                          return;
                                      }
                                      resultBody.innerHTML = logs.map(log => `
                                          <tr class="border-b transition-colors hover:bg-muted/50">
                                              <td class="p-4 align-middle">${log.deviceName ?? '-'}</td>
                                              <td class="p-4 align-middle font-mono text-xs">${log.tagAddress ?? '-'}</td>
                                              <td class="p-4 align-middle">${log.tagRemark ?? '-'}</td>
                                              <td class="p-4 align-middle font-medium">${log.value ?? '-'}</td>
                                              <td class="p-4 align-middle text-muted-foreground">${log.timestamp ? new Date(log.timestamp).toLocaleString('zh-CN') : '-'}</td>
                                              <td class="p-4 align-middle text-xs text-muted-foreground">${formatParams(log.parameters)}</td>
                                          </tr>`).join('');
                                  };

                                  const toChinaTime = (value) => value ? `${value}:00+08:00` : null;

                                  form.addEventListener('submit', async (e) => {
                                      e.preventDefault();
                                      statusEl.textContent = '正在查询...';

                                      const rawValues = form.values.value.trim();
                                      const valuesArray = rawValues
                                          ? rawValues.split(',').map(v => v.trim()).filter(v => v.length > 0)
                                          : null;

                                      try {
                                          const payload = {
                                              deviceName: form.deviceName.value || null,
                                              tagRemark: form.tagRemark.value || null,
                                              values: valuesArray,
                                              startTime: toChinaTime(form.startTime.value),
                                              endTime: toChinaTime(form.endTime.value)
                                          };

                                          const response = await fetch(endpoint, {
                                              method: 'POST',
                                              headers: { 'Content-Type': 'application/json' },
                                              body: JSON.stringify(payload)
                                          });

                                          if (!response.ok) throw new Error(`HTTP ${response.status}`);

                                          const data = await response.json();
                                          renderRows(data);
                                          statusEl.textContent = `查询成功，共找到 ${data.length} 条记录。`;
                                      } catch (err) {
                                          resultBody.innerHTML = '<tr><td colspan="6" class="p-4 text-center text-destructive">请求失败</td></tr>';
                                          statusEl.textContent = `请求失败：${err.message}`;
                                      }
                                  });

                                  initDefaults();
                              </script>
                          </body>
                          </html>
                          """;

        await Send.StringAsync(htmlContent, contentType: "text/html; charset=utf-8", cancellation: ct);
    }
}