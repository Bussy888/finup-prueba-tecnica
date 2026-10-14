import { statusLabels } from "@/lib/claims";
import { formatDateTime } from "@/lib/format";
import type { StatusHistoryItem } from "@/lib/types";

export function HistoryList({ history }: { history: StatusHistoryItem[] }) {
  const ordered = [...history].sort(
    (a, b) => new Date(a.changedAt).getTime() - new Date(b.changedAt).getTime(),
  );

  return (
    <ol className="timeline">
      {ordered.map((item, index) => (
        <li key={`${item.changedAt}-${index}`}>
          <p className="timeline-title">
            {item.fromStatus
              ? `${statusLabels[item.fromStatus]} → ${statusLabels[item.toStatus]}`
              : statusLabels[item.toStatus]}
          </p>
          <time dateTime={item.changedAt}>{formatDateTime(item.changedAt)}</time>
          {item.comment && <p className="timeline-comment">{item.comment}</p>}
        </li>
      ))}
    </ol>
  );
}
