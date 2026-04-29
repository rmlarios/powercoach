'use client';

import { useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Skeleton } from '@/components/ui/skeleton';
import {
  ArrowLeft,
  CheckCircle,
  XCircle,
  User,
  Mail,
  Phone,
  MapPin,
  Dumbbell,
  Target,
  MessageSquare,
} from 'lucide-react';
import { useApplication, useApproveApplication, useRejectApplication } from '@/hooks';
import { ApplicationStatus } from '@/types';
import { format } from 'date-fns';

function statusColor(status: ApplicationStatus) {
  switch (status) {
    case 'Pending': return 'bg-yellow-100 text-yellow-800';
    case 'UnderReview': return 'bg-blue-100 text-blue-800';
    case 'Accepted': return 'bg-green-100 text-green-800';
    case 'Rejected': return 'bg-red-100 text-red-800';
    case 'Withdrawn': return 'bg-slate-100 text-slate-800';
    default: return 'bg-slate-100 text-slate-800';
  }
}

export default function ApplicationDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();

  const { data: app, isLoading } = useApplication(id);
  const approveApp = useApproveApplication();
  const rejectApp = useRejectApplication();

  const [rejectOpen, setRejectOpen] = useState(false);
  const [rejectReason, setRejectReason] = useState('');
  const [approveOpen, setApproveOpen] = useState(false);
  const [approveNotes, setApproveNotes] = useState('');

  const handleApprove = async () => {
    await approveApp.mutateAsync({ id, notes: approveNotes || undefined });
    setApproveOpen(false);
    router.push('/applications');
  };

  const handleReject = async () => {
    if (!rejectReason.trim()) return;
    await rejectApp.mutateAsync({ id, reason: rejectReason });
    setRejectOpen(false);
    router.push('/applications');
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-[400px] w-full" />
      </div>
    );
  }

  if (!app) {
    return (
      <div className="text-center py-12">
        <p className="text-slate-500">Application not found.</p>
        <Button variant="link" onClick={() => router.push('/applications')}>
          Back to applications
        </Button>
      </div>
    );
  }

  const isPending = app.status === 'Pending' || app.status === 'UnderReview';

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" onClick={() => router.push('/applications')}>
            <ArrowLeft className="h-4 w-4 mr-1" /> Back
          </Button>
          <div>
            <h1 className="text-2xl font-bold text-slate-900">{app.fullName}</h1>
            <p className="text-sm text-slate-500">
              Applied {format(new Date(app.createdAt), 'MMMM d, yyyy')}
            </p>
          </div>
          <Badge className={statusColor(app.status)}>{app.status}</Badge>
        </div>

        {isPending && (
          <div className="flex gap-2">
            <Button
              variant="outline"
              onClick={() => setRejectOpen(true)}
              className="text-red-600 border-red-200 hover:bg-red-50"
            >
              <XCircle className="h-4 w-4 mr-2" />
              Reject
            </Button>
            <Button onClick={() => setApproveOpen(true)}>
              <CheckCircle className="h-4 w-4 mr-2" />
              Approve
            </Button>
          </div>
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Personal Info */}
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Personal Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <InfoRow icon={<User className="h-4 w-4" />} label="Name" value={app.fullName} />
            <InfoRow icon={<Mail className="h-4 w-4" />} label="Email" value={app.email} />
            {app.phone && <InfoRow icon={<Phone className="h-4 w-4" />} label="Phone" value={app.phone} />}
            {app.country && <InfoRow icon={<MapPin className="h-4 w-4" />} label="Country" value={app.country} />}
            {app.age && <InfoRow label="Age" value={String(app.age)} />}
            {app.gender && <InfoRow label="Gender" value={app.gender} />}
          </CardContent>
        </Card>

        {/* Training Info */}
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Training Background</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {app.trainingExperience && (
              <InfoRow icon={<Dumbbell className="h-4 w-4" />} label="Experience" value={app.trainingExperience} />
            )}
            {(app.currentSquat || app.currentBench || app.currentDeadlift) && (
              <div>
                <p className="text-sm font-medium text-slate-500 mb-2">Current Lifts</p>
                <div className="grid grid-cols-3 gap-3">
                  <LiftCard label="Squat" value={app.currentSquat} />
                  <LiftCard label="Bench" value={app.currentBench} />
                  <LiftCard label="Deadlift" value={app.currentDeadlift} />
                </div>
                {app.totalLifts && (
                  <p className="mt-2 text-sm text-slate-600">
                    Total: <span className="font-semibold">{app.totalLifts} kg</span>
                  </p>
                )}
              </div>
            )}
          </CardContent>
        </Card>

        {/* Goals & Motivation */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle className="text-base">Goals & Motivation</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {app.goals && (
              <InfoRow icon={<Target className="h-4 w-4" />} label="Goals" value={app.goals} />
            )}
            {app.motivation && (
              <InfoRow icon={<MessageSquare className="h-4 w-4" />} label="Motivation" value={app.motivation} />
            )}
            {app.message && (
              <div>
                <p className="text-sm font-medium text-slate-500 mb-1">Message</p>
                <p className="text-sm text-slate-700 whitespace-pre-wrap">{app.message}</p>
              </div>
            )}
            {app.referralSource && (
              <InfoRow label="Referral Source" value={app.referralSource} />
            )}
          </CardContent>
        </Card>

        {/* Review Info (if already reviewed) */}
        {(app.coachNotes || app.rejectionReason || app.reviewedAt) && (
          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle className="text-base">Review Details</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              {app.reviewedAt && (
                <InfoRow
                  label="Reviewed At"
                  value={format(new Date(app.reviewedAt), 'MMMM d, yyyy HH:mm')}
                />
              )}
              {app.coachNotes && <InfoRow label="Coach Notes" value={app.coachNotes} />}
              {app.rejectionReason && (
                <div>
                  <p className="text-sm font-medium text-red-500 mb-1">Rejection Reason</p>
                  <p className="text-sm text-slate-700">{app.rejectionReason}</p>
                </div>
              )}
            </CardContent>
          </Card>
        )}
      </div>

      {/* Approve Dialog */}
      <Dialog open={approveOpen} onOpenChange={setApproveOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Approve Application</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-slate-600">
            Approving will automatically create an athlete profile for <strong>{app.fullName}</strong>.
          </p>
          <div className="grid gap-2">
            <Label htmlFor="approveNotes">Notes (optional)</Label>
            <Input
              id="approveNotes"
              value={approveNotes}
              onChange={(e) => setApproveNotes(e.target.value)}
              placeholder="Any notes about this approval..."
            />
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setApproveOpen(false)}>Cancel</Button>
            <Button onClick={handleApprove} disabled={approveApp.isPending}>
              {approveApp.isPending ? 'Approving...' : 'Approve & Create Athlete'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Reject Dialog */}
      <Dialog open={rejectOpen} onOpenChange={setRejectOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Reject Application</DialogTitle>
          </DialogHeader>
          <div className="grid gap-2">
            <Label htmlFor="rejectReason">Reason *</Label>
            <Input
              id="rejectReason"
              value={rejectReason}
              onChange={(e) => setRejectReason(e.target.value)}
              placeholder="Reason for rejection..."
            />
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setRejectOpen(false)}>Cancel</Button>
            <Button
              variant="destructive"
              onClick={handleReject}
              disabled={!rejectReason.trim() || rejectApp.isPending}
            >
              {rejectApp.isPending ? 'Rejecting...' : 'Reject'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function InfoRow({
  icon,
  label,
  value,
}: {
  icon?: React.ReactNode;
  label: string;
  value: string;
}) {
  return (
    <div className="flex items-start gap-3">
      {icon && <span className="mt-0.5 text-slate-400">{icon}</span>}
      <div>
        <p className="text-sm font-medium text-slate-500">{label}</p>
        <p className="text-sm text-slate-900">{value}</p>
      </div>
    </div>
  );
}

function LiftCard({ label, value }: { label: string; value?: number }) {
  return (
    <div className="rounded-lg bg-slate-50 p-3 text-center">
      <p className="text-xs text-slate-500">{label}</p>
      <p className="text-lg font-semibold text-slate-900">
        {value ? `${value} kg` : '—'}
      </p>
    </div>
  );
}
