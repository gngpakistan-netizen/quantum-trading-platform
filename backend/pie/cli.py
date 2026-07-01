"""Project Intelligence Engine (PIE) — CLI Interface.

Usage:
    pie status                    # Project health summary
    pie requirements              # List all requirements
    pie todo list                 # List all open TODOs
    pie todo add "text"           # Add a TODO
    pie todo done <id>            # Mark TODO done
    pie audit latest              # Latest audit score
    pie validation run --stream mathematical
    pie release create v4.0.1     # Create release
"""

import sys
import json
from datetime import date
from typing import Optional

from backend.pie.models import (
    Requirement, TodoItem, AuditScore, ValidationRun,
    FormulaEntry, Release, RTM_REQUIREMENTS, PIE_TODOS,
)


def cmd_status():
    """Display project health summary."""
    req_count = len(RTM_REQUIREMENTS)
    req_done = sum(1 for r in RTM_REQUIREMENTS.values() if r.status == "validated")
    req_in_prog = sum(1 for r in RTM_REQUIREMENTS.values() if r.status == "implemented")
    todo_done = sum(1 for t in PIE_TODOS if t.status == "completed")
    todo_open = sum(1 for t in PIE_TODOS if t.status != "completed")

    print("╔══════════════════════════════════════════════════════════╗")
    print("║  XAUUSD Quantum Platform — Project Health               ║")
    print(f"║  {date.today().isoformat()}                                 ║")
    print("╠══════════════════════════════════════════════════════════╣")
    print(f"║  REQUIREMENTS: {req_count:2d} total  {req_done:2d} done  {req_in_prog:2d} in prog  {req_count - req_done - req_in_prog:2d} pending  ║")
    print(f"║  TODOS:        {todo_done + todo_open:2d} total  {todo_done:2d} done  {todo_open:2d} open                              ║")
    print(f"║  AUDIT:        --/100  (no runs yet)                     ║")
    print(f"║  FORMULAS:     0 verified                                ║")
    print(f"║  BUILD:        setup in progress                         ║")
    print("╚══════════════════════════════════════════════════════════╝")


def cmd_requirements(status_filter: Optional[str] = None):
    """List requirements with optional status filter."""
    items = RTM_REQUIREMENTS.values()
    if status_filter:
        items = [r for r in items if r.status == status_filter]

    if not items:
        print("No requirements found.")
        return

    print(f"{'ID':<12} {'Status':<14} {'Origin':<10} {'Description':<50}")
    print("-" * 86)
    for r in sorted(items, key=lambda x: x.id):
        desc = r.description[:47] + "..." if len(r.description) > 50 else r.description
        print(f"{r.id:<12} {r.status:<14} {r.origin:<10} {desc:<50}")


def cmd_todo_list():
    """List all open TODOs."""
    open_todos = [t for t in PIE_TODOS if t.status != "completed"]
    if not open_todos:
        print("No open TODOs.")
        return

    print(f"{'ID':<8} {'Priority':<10} {'Status':<14} {'Content'}")
    print("-" * 80)
    for t in open_todos:
        tid = str(t.id)[:8]
        print(f"{tid:<8} {t.priority:<10} {t.status:<14} {t.content}")


def cmd_todo_add(content: str, priority: str = "medium"):
    """Add a new TODO."""
    todo = TodoItem(content=content, priority=priority)
    PIE_TODOS.append(todo)
    print(f"Added TODO [{str(todo.id)[:8]}]: {content}")


def cmd_todo_done(todo_id: str):
    """Mark a TODO as completed."""
    for t in PIE_TODOS:
        if str(t.id).startswith(todo_id):
            t.status = "completed"
            t.completed_at = __import__("datetime").datetime.utcnow()
            print(f"Completed TODO: {t.content}")
            return
    print(f"TODO not found: {todo_id}")


def cmd_audit_latest():
    """Display latest audit score."""
    print("No audit runs yet. Use `pie audit run` to execute.")


def cmd_release_create(version: str, notes: Optional[str] = None):
    """Create a new release entry."""
    release = Release(version=version, notes=notes)
    print(f"Created release: {release.version} ({release.release_date})")


def main():
    args = sys.argv[1:]
    if not args:
        print("Usage: pie <command> [options]")
        print("Commands: status, requirements, todo, audit, validation, release")
        return 1

    cmd = args[0]

    if cmd == "status":
        cmd_status()
    elif cmd == "requirements":
        status_filter = None
        for i, a in enumerate(args[1:], 1):
            if a == "--status" and i < len(args):
                status_filter = args[i + 1]
        cmd_requirements(status_filter)
    elif cmd == "todo":
        if len(args) < 2:
            cmd_todo_list()
        elif args[1] == "list":
            cmd_todo_list()
        elif args[1] == "add":
            content = " ".join(args[2:])
            cmd_todo_add(content)
        elif args[1] == "done" and len(args) >= 3:
            cmd_todo_done(args[2])
    elif cmd == "audit":
        if len(args) >= 2 and args[1] == "latest":
            cmd_audit_latest()
    elif cmd == "release":
        if len(args) >= 2 and args[1] == "create" and len(args) >= 3:
            cmd_release_create(args[2])
    else:
        print(f"Unknown command: {cmd}")
        return 1

    return 0


if __name__ == "__main__":
    sys.exit(main())
