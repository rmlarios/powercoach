'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useParams } from 'next/navigation';
import { StatusBadge } from '@/components/common';
import { useCheckInById, useReviewCheckIn } from '@/hooks';
import { useAuth } from '@/providers';
import { format } from 'date-fns';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { ArrowLeft, Save, Activity, Moon, Brain, Pizza, Dumbbell } from 'lucide-react';
import { toast } from 'sonner';

export default function CheckInDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const { user } = useAuth();
  
  const { data: checkIn, isLoading } = useCheckInById(id, user?.id || '');
  const reviewMutation = useReviewCheckIn();
  
  const [feedback, setFeedback] = useState('');
  
  // Set initial feedback if it already exists
  if (checkIn?.coachFeedback && !feedback && !reviewMutation.isSuccess) {
    setFeedback(checkIn.coachFeedback);
  }

  const handleReview = async () => {
    if (!feedback.trim()) {
      toast.error('Feedback cannot be empty');
      return;
    }
    
    try {
      await reviewMutation.mutateAsync({
        checkInId: id,
        data: {
          coachId: user?.id || '',
          feedback
        }
      });
      toast.success('Check-in reviewed successfully!');
      router.push('/check-ins');
    } catch (error) {
      console.error(error);
      toast.error('Failed to submit review');
    }
  };

  if (isLoading) return <div className="p-8 text-center text-slate-500">Loading check-in data...</div>;
  if (!checkIn) return <div className="p-8 text-center text-slate-500">Check-in not found.</div>;

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => router.push('/check-ins')}>
            <ArrowLeft className="w-5 h-5" />
          </Button>
          <div>
            <h1 className="text-2xl font-bold text-slate-900">{checkIn.athleteName}</h1>
            <p className="text-slate-500">{format(new Date(checkIn.checkInDate), 'MMMM d, yyyy')}</p>
          </div>
        </div>
        <StatusBadge 
          status={checkIn.reviewedAt ? 'active' : 'pending'} 
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Core Stats */}
        <div className="col-span-1 md:col-span-3 lg:col-span-1 space-y-6">
          <div className="p-5 bg-white rounded-xl border border-slate-200 shadow-sm">
            <h2 className="text-sm font-semibold text-slate-500 uppercase tracking-wider mb-4">Body Metrics</h2>
            <div className="flex items-end gap-2 mb-2 pb-6 border-b border-slate-100">
              <span className="text-3xl font-bold text-slate-900">{checkIn.weight ?? '--'}</span>
              <span className="text-slate-500 font-medium pb-1">{checkIn.weightUnit || 'kg'}</span>
            </div>
            
            <div className="space-y-4 pt-4">
              <div className="flex justify-between items-center pb-3 border-b border-slate-100">
                <div className="flex items-center text-slate-600 gap-2"><Activity className="w-4 h-4 text-blue-500"/> Energy</div>
                <span className="font-semibold">{checkIn.energyLevel ?? '--'}/10</span>
              </div>
              <div className="flex justify-between items-center pb-3 border-b border-slate-100">
                <div className="flex items-center text-slate-600 gap-2"><Brain className="w-4 h-4 text-purple-500"/> Stress</div>
                <span className="font-semibold">{checkIn.stressLevel ?? '--'}/10</span>
              </div>
              <div className="flex justify-between items-center pb-3 border-b border-slate-100">
                <div className="flex items-center text-slate-600 gap-2"><Moon className="w-4 h-4 text-indigo-500"/> Sleep</div>
                <div className="text-right">
                  <span className="font-semibold block">{checkIn.sleepQuality ?? '--'}/10</span>
                  <span className="text-xs text-slate-400">{checkIn.sleepHours ?? '--'} hrs</span>
                </div>
              </div>
              <div className="flex justify-between items-center pb-3 border-b border-slate-100">
                <div className="flex items-center text-slate-600 gap-2"><Pizza className="w-4 h-4 text-orange-500"/> Nutrition</div>
                <span className="font-semibold">{checkIn.nutritionAdherence ?? '--'}%</span>
              </div>
              <div className="flex justify-between items-center">
                <div className="flex items-center text-slate-600 gap-2"><Dumbbell className="w-4 h-4 text-green-500"/> Training</div>
                <span className="font-semibold">{checkIn.trainingAdherence ?? '--'}%</span>
              </div>
            </div>
          </div>
        </div>

        {/* Notes & Feedback */}
        <div className="col-span-1 md:col-span-3 lg:col-span-2 space-y-6">
          <div className="p-5 bg-white rounded-xl border border-slate-200 shadow-sm">
            <h2 className="text-sm font-semibold text-slate-500 uppercase tracking-wider mb-4">Athlete&apos;s Notes</h2>
            <div className="p-4 bg-slate-50 rounded-lg text-slate-700 whitespace-pre-wrap min-h-[100px]">
              {checkIn.notes || <span className="text-slate-400 italic">No notes provided...</span>}
            </div>
          </div>
          
          {checkIn.photoUrls && checkIn.photoUrls.length > 0 && (
            <div className="p-5 bg-white rounded-xl border border-slate-200 shadow-sm">
              <h2 className="text-sm font-semibold text-slate-500 uppercase tracking-wider mb-4">Progress Photos</h2>
              <div className="flex gap-4 overflow-x-auto pb-2">
                {checkIn.photoUrls.map((url, i) => (
                  <div key={i} className="w-32 h-32 rounded-lg bg-slate-100 border border-slate-200 flex items-center justify-center flex-shrink-0">
                    <span className="text-xs text-slate-400">Photo {i+1}</span>
                  </div>
                ))}
              </div>
            </div>
          )}

          <div className="p-5 bg-white rounded-xl border border-blue-200 shadow-sm ring-1 ring-blue-50">
            <h2 className="text-sm font-semibold text-blue-600 uppercase tracking-wider mb-4">Coach Feedback</h2>
            
            {checkIn.reviewedAt && !reviewMutation.isPending && !reviewMutation.isSuccess ? (
              <div className="p-4 bg-blue-50 rounded-lg text-slate-800 whitespace-pre-wrap">
                {checkIn.coachFeedback}
              </div>
            ) : (
              <div className="space-y-4">
                <Textarea 
                  placeholder="Provide feedback and next steps for the athlete..."
                  className="min-h-[150px] resize-none"
                  value={feedback}
                  onChange={(e) => setFeedback(e.target.value)}
                />
                <div className="flex justify-end">
                  <Button 
                    onClick={handleReview} 
                    disabled={reviewMutation.isPending || !feedback.trim()}
                    className="gap-2"
                  >
                    <Save className="w-4 h-4" />
                    {reviewMutation.isPending ? 'Saving...' : 'Submit Review'}
                  </Button>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
