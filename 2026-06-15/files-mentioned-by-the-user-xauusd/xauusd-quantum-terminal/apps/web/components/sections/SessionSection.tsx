'use client';

export function SessionSection() {
  const sessions = [
    { name: 'ASIA', interval: '00:00–09:00', active: false },
    { name: 'LONDON', interval: '08:00–17:00', active: true },
    { name: 'NY', interval: '13:00–22:00', active: false },
    { name: 'OFF-HOURS', interval: '22:00–00:00', active: false },
  ];

  return (
    <>
      <div className="panel-header">SESSION</div>
      <div className="p-2 space-y-1">
        {sessions.map((s) => (
          <div key={s.name} className="data-row">
            <span className="data-label">{s.name}</span>
            <span className="text-2xs text-terminal-muted">{s.interval}</span>
            <span className={`inline-block w-2 h-2 rounded-full ${s.active ? 'bg-terminal-green' : 'bg-terminal-muted'}`} />
          </div>
        ))}
      </div>
    </>
  );
}
