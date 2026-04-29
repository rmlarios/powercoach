'use client';

import { MessageCircle } from 'lucide-react';

interface CoachNoteProps {
  text: string;
}

/**
 * CoachNote — chat-bubble style inline note from the coach.
 *
 * KILLER FEATURE: No competitor (Strong / Hevy / JEFIT) shows real-time
 * coach notes inside the workout tracker. This is the coach-in-your-pocket
 * differentiator.
 *
 * Design:
 * - Soft blue bubble (not intrusive, consistent with paper feel)
 * - Mini coach avatar icon (MessageCircle)
 * - Compact — fits under exercise name without pushing layout
 */
export function CoachNote({ text }: CoachNoteProps) {
  return (
    <div className="flex items-start gap-1.5 mt-1.5 mb-0.5">
      {/* Coach icon */}
      <div className="flex-shrink-0 w-5 h-5 rounded-full bg-blue-100 flex items-center justify-center mt-0.5">
        <MessageCircle className="w-3 h-3 text-blue-500" />
      </div>

      {/* Bubble */}
      <div className="relative flex-1 bg-blue-50 border border-blue-100 rounded-lg rounded-tl-sm px-2.5 py-1.5 max-w-[90%]">
        <p className="text-xs text-blue-700 leading-relaxed">
          {text}
        </p>
        <span className="text-[10px] text-blue-400 mt-0.5 block">Coach</span>
      </div>
    </div>
  );
}
